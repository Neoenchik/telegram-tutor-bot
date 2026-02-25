namespace TutorBot.Infrastructure.Extensions;

/// <summary>
/// Набор расширений для <see cref="DateTime"/>, упрощающих работу с часовыми поясами.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Преобразует момент времени из UTC в указанный часовой пояс.
    /// </summary>
    /// <param name="utcTime">
    /// Входное время; если <see cref="DateTime.Kind"/> не равно <see cref="DateTimeKind.Utc"/>,
    /// оно будет предварительно приведено при помощи <see cref="DateTime.ToUniversalTime"/>.
    /// </param>
    /// <param name="timeZoneId">
    /// Идентификатор целевого часового пояса (например, <c>"Europe/Moscow"</c>).
    /// </param>
    /// <returns>Время, сконвертированное в указанный часовой пояс.</returns>
    /// <exception cref="TimeZoneNotFoundException">
    /// Если передан неверный идентификатор часового пояса.
    /// </exception>
    /// <exception cref="InvalidTimeZoneException">
    /// Если информация о часовом поясе повреждена или недоступна.
    /// </exception>
    public static DateTime ToTimeZone(this DateTime utcTime, string timeZoneId)
    {
        if (utcTime.Kind != DateTimeKind.Utc)
            utcTime = utcTime.ToUniversalTime();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
    }
}
