public static class DateTimeHelper
{
    public static DateTime CombineDatumEnTijd(DateTime datum, string tijd)
    {
        TimeSpan parsedTijd = TimeSpan.Parse(tijd);
        return datum.Date.Add(parsedTijd);
    }
}
