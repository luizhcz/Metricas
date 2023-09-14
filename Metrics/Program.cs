using Metrics;
using Metrics.HostedServices;
using Metrics.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddScoped<ICache, Cache>();
                services.AddScoped<IMetricsClient, MetricsClient>();
                services.AddScoped<IDateMetrics, DateMetrics>();
                services.AddHostedService<RabbitMQHostedService>();
            }).Build().Run();