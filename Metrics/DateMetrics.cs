using Metrics.Interfaces;

namespace Metrics;

public class DateMetrics : IDateMetrics
{
    public double CalculateMaxInterval(double lastInterval, DateTime lastDate, DateTime date) 
    {
        if (lastInterval == 0 && lastDate == date)
            return 0;

        if (lastInterval == 0 && lastDate != date)
            return (date - lastDate).TotalMinutes;

        return (date - lastDate).TotalMinutes > lastInterval ? (date - lastDate).TotalMinutes : lastInterval;
    }

    public double CalculateMinInterval(double lastInterval, DateTime lastDate, DateTime date)
    {
        if (lastInterval == 0 && lastDate == date)
            return 0;

        if (lastInterval == 0 && lastDate != date)
            return (date - lastDate).TotalMinutes;

        return (date - lastDate).TotalMinutes < lastInterval ? (date - lastDate).TotalMinutes : lastInterval;
    }

    public (double media, double standardDeviation) CalcMediaAndStandardDeviation(List<DateTime> dates) 
    {
        if (dates.Count > 0) 
        {
            double totalMinutes = 0;

            foreach (var date in dates)
            {
                totalMinutes += date.TimeOfDay.TotalMinutes;
            }

            double mediaByMinutes = totalMinutes / dates.Count;
            double sumOfSquaresOfDifferences = dates.Sum(date => Math.Pow(date.TimeOfDay.TotalMinutes - mediaByMinutes, 2));
            double desvioPadrao = Math.Sqrt(sumOfSquaresOfDifferences / (dates.Count - 1));

            return (mediaByMinutes, desvioPadrao);
        }

        return (0, 0);
    }

    public bool IsLogTimeExpired(DateTime StartDate, DateTime FinalDate, int TimeSendBatchByMinutes)
    {
        TimeSpan diferenca = FinalDate - StartDate;

        return diferenca.TotalMinutes > TimeSendBatchByMinutes;
    }

    public int LogTime(DateTime StartDate, DateTime FinalDate) 
    {
        return (int)((FinalDate - StartDate).TotalMinutes);
    } 
}