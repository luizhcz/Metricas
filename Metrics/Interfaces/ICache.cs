namespace Metrics.Interfaces;

public interface ICache
{
    T? TryGetValue<T>(object Key);
    void SetValue<T>(object Key, T value);
    void DeleteValue(object Key);
}