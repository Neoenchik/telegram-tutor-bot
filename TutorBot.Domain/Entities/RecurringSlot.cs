namespace TutorBot.Domain.Entities;

/// <summary>
/// Повторяющийся слот в расписании репетитора (задаётся один раз для конкретного дня недели).
/// </summary>
public class RecurringSlot
{
    /// <summary>Первичный ключ слота.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// День недели по ISO 8601 (0 = понедельник … 6 = воскресенье).
    /// </summary>
    public int Weekday { get; set; }

    /// <summary>Время начала слота (UTC, без учёта даты).</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>Продолжительность занятия в минутах (по умолчанию 60).</summary>
    public int DurationMinutes { get; set; } = 60;

    /// <summary>Дата и время создания записи (UTC).</summary>
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
}