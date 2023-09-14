using Metrics.Interfaces;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Metrics.HostedServices;

public class RabbitMQHostedService : IHostedService, IDisposable
{
    private readonly ConnectionFactory connectionFactory;
    private readonly ICache _cacheMetrics;
    private readonly IMetricsClient _metricsClient;
    private readonly IDateMetrics _dateMetrics;

    private readonly IConnection _connection;
    private readonly IModel _channel;

    private readonly string Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!;
    private readonly int Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PORT")!);
    private readonly string User = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER")!;
    private readonly string Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")!;
    private readonly string queueName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_QUEUE")!;
    private readonly int TimeSendBatchByMinutes = Convert.ToInt32(Environment.GetEnvironmentVariable("TIME_SEND_BATCH_MINUTES")!);

    public RabbitMQHostedService(
        ICache cache,
        IMetricsClient metricsClient,
        IDateMetrics dateMetrics)
    {
        connectionFactory = new ConnectionFactory
        {
            HostName = Host,
            Port = Port,
            UserName = User,
            Password = Password
        };

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _cacheMetrics = cache ?? throw new ArgumentNullException(nameof(cache));
        _metricsClient = metricsClient ?? throw new ArgumentNullException(nameof(metricsClient));
        _dateMetrics = dateMetrics ?? throw new ArgumentNullException(nameof(dateMetrics));
    }

    private async Task ReceivedMetrics(
        BasicDeliverEventArgs args,
        IModel? channel)
    {
        var body = args.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        try
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };
            var jsonData = JsonSerializer.Deserialize<File>(message, options)!;
            string filename = jsonData.Filename!;

            // Extrair informações do nome do arquivo usando expressão regular
            string pattern = @"^([a-zA-Z0-9]+)_([a-zA-Z0-9]+)_([0-9]+).txt$";
            var match = Regex.Match(filename, pattern);

            if (match.Success)
            {
                var nameMachine = match.Groups[1].Value;
                var nameApplication = match.Groups[2].Value;
                var dateFile = match.Groups[3].Value;
                DateTime date = DateTime.Now; //o valor do exemplo "2021101131559" não segue um padrão "yyyyMMddHHmmss"

                var logMetrics = _cacheMetrics.TryGetValue<LogMetrics>(nameApplication);

                //Recupera um item do Cache
                if (logMetrics != default)
                {
                    logMetrics!.MinIntervalByMinutes = _dateMetrics.CalculateMinInterval(logMetrics.LastIntervalByMinutes, logMetrics.LastDate, date);
                    logMetrics!.MaxIntervalByMinutes = _dateMetrics.CalculateMaxInterval(logMetrics.LastIntervalByMinutes, logMetrics.LastDate, date);
                    logMetrics!.LastIntervalByMinutes = (date - logMetrics.LastDate).TotalMinutes;
                    logMetrics!.Count++;
                    logMetrics!.LastDate = date;
                    logMetrics!.InfoFiles!.Add(new Info { Machine = nameMachine, Application = nameApplication, Date = dateFile, ReceivedDate = date });

                    if (_dateMetrics.IsLogTimeExpired(logMetrics.InitialDate, date, TimeSendBatchByMinutes))
                    {
                        // Log da métrica
                        Console.WriteLine($"count = {logMetrics.Count} for file {nameApplication} {date}");

                        var MathDate = _dateMetrics.CalcMediaAndStandardDeviation(logMetrics.InfoFiles!.Select(i => i.ReceivedDate).ToList());
                        logMetrics.AverageIntervalBetweenFiles = MathDate.media;
                        logMetrics.StandardDeviation = MathDate.standardDeviation;
                        logMetrics.LogTimeByMinutes = _dateMetrics.LogTime(logMetrics.InitialDate, logMetrics.LastDate);

                        //Log simulando o que seria enviado para o endpoint
                        Console.WriteLine("{");
                        Console.WriteLine($"  'Count' : '{logMetrics.Count}' ");
                        Console.WriteLine($"  'Average' : '{logMetrics.AverageIntervalBetweenFiles}' ");
                        Console.WriteLine($"  'MaxInterval' : '{logMetrics.MaxIntervalByMinutes}' ");
                        Console.WriteLine($"  'MinInterval' : '{logMetrics.MinIntervalByMinutes}' ");
                        Console.WriteLine($"  'LogTimeByMinutes' : '{logMetrics.LogTimeByMinutes}' ");
                        Console.WriteLine($"  'StartDate' : '{logMetrics.InitialDate}' ");
                        Console.WriteLine($"  'LastDate' : '{logMetrics.LastDate}' ");
                        Console.WriteLine("  'Files' : { ");
                        logMetrics.InfoFiles.ForEach(file =>
                        {
                            Console.WriteLine($"      'Machine' : '{file.Machine}' ");
                            Console.WriteLine($"      'Application' : '{file.Application}' ");
                            Console.WriteLine($"      'Date' : '{file.Date}' ");
                        });
                        Console.WriteLine("   }");
                        Console.WriteLine("}");

                        // Envia a metrica para o endpoint informado
                        //await _metricsClient.PostJsonAsync("", logMetrics);

                        //Remove um item do cache
                        _cacheMetrics.DeleteValue(nameApplication);
                    }
                    else
                    {
                        //Adiciona um item ao cache
                        _cacheMetrics.SetValue(nameApplication, logMetrics);

                        // Log da métrica
                        Console.WriteLine($"count = {logMetrics.Count} for file {nameApplication} {date}");
                    }
                }
                else
                {
                    logMetrics = new LogMetrics()
                    {
                        Count = 1,
                        AverageIntervalBetweenFiles = 0,
                        StandardDeviation = 0,
                        LogTimeByMinutes = 0,
                        InitialDate = date,
                        LastDate = date,
                        MaxIntervalByMinutes = 0,
                        MinIntervalByMinutes = 0,
                        LastIntervalByMinutes = 0,
                        InfoFiles = new List<Info> { new Info { Machine = nameMachine, Application = nameApplication, Date = dateFile, ReceivedDate = date } }
                    };

                    //Adiciona um item ao cache
                    _cacheMetrics.SetValue(nameApplication, logMetrics);

                    // Log da métrica
                    Console.WriteLine($"count = {logMetrics.Count} for file {nameApplication} {date}");
                }
            }
            else
            {
                Console.WriteLine($"File name Invalid: {filename}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }

        channel!.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _channel.QueueDeclare(queue: queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) => await ReceivedMetrics(ea, _channel);

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        //Console.WriteLine("Aguardando mensagens. Pressione Enter para sair.");
        //Console.ReadLine();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Close();
        _connection.Close();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}