using System.Text;
using Metrics.Interfaces;

namespace Metrics;

public class MetricsClient : IMetricsClient
{
    public async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data)
    {
        using var client = new HttpClient();
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await client.PostAsync(url, content);
    }
}
