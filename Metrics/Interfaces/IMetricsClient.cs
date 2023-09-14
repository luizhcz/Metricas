namespace Metrics.Interfaces;

public interface IMetricsClient
{
    Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data);
}