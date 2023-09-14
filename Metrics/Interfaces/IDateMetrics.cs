namespace Metrics.Interfaces;

public interface IDateMetrics
{
    double CalculateMinInterval(double lastInterval, DateTime lastDate, DateTime date);
    double CalculateMaxInterval(double lastInterval, DateTime lastDate, DateTime date);
    (double media, double standardDeviation) CalcMediaAndStandardDeviation(List<DateTime> files);
    int LogTime(DateTime StartDate, DateTime FinalDate);
    bool IsLogTimeExpired(DateTime StartDate, DateTime FinalDate, int TimeSendBatchByMinutes);
}