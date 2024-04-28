namespace MqttBridge.Models;

public static class TimeExtensions
{
    public static DateTime ToSwissTime(this DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _swissTimeZone);
    }

    private static TimeZoneInfo _swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");
}