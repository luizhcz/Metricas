namespace Metrics
{
    public static class DateHelper
    {
        public static DateTime ToDateTime(this string date) 
        {
            int year = Convert.ToInt32(date[..4]);
            int month = Convert.ToInt32(date.Substring(4, 2));
            int day = Convert.ToInt32(date.Substring(6, 2));
            int hour = Convert.ToInt32(date.Substring(8, 2));
            int minute = Convert.ToInt32(date.Substring(10, 2));
            int second = date.Length == 13 
                ? Convert.ToInt32(date.Substring(12, 1)) : Convert.ToInt32(date.Substring(12, 2));

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}