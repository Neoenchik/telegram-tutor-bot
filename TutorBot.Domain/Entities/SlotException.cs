namespace TutorBot.Domain.Entities;

/// <summary>
/// Исключение в расписании репетитора: блокирует конкретный день или часть дня.
/// </summary>
public class SlotException
{
    /// <summary>Первичный ключ исключения.</summary>
    public Guid Id { get; set; }

    /// <summary>Дата, на которую распространяется исключение.</summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Если <c>true</c> — весь день недоступен;
    /// если <c>false</c> — недоступен период до <see cref="UntilTime"/>.
    /// </summary>
    public bool FullDay { get; set; } = true;

    /// <summary>
    /// Время окончания блокировки (UTC). Актуально только если <see cref="FullDay"/> == <c>false</c>.
    /// </summary>
    public TimeSpan? UntilTime { get; set; }

    /// <summary>Дата и время создания записи (UTC).</summary>
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
}