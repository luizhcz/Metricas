public sealed class LogMetrics 
{
    public int Count { get; set; }
    public DateTime InitialDate { get; set; }
    public DateTime LastDate { get; set; }
    public double MaxIntervalByMinutes { get; set; }
    public double MinIntervalByMinutes { get; set; }
    public double LastIntervalByMinutes { get; set; }
    public double AverageIntervalBetweenFiles { get; set; }
    public double StandardDeviation { get; set; }
    public int LogTimeByMinutes { get; set; }
    public List<Info>? InfoFiles { get; set; }
}
