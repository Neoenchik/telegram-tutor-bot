namespace TutorBot.Domain.Entities;

/// <summary>
/// Запись на занятие, созданная учеником и ожидающая подтверждения репетитора.
/// </summary>
public class Lesson
{
    /// <summary>Первичный ключ урока.</summary>
    public Guid Id { get; set; }

    /// <summary>Telegram‑ID ученика (внешний ключ к <see cref="User.Id"/>).</summary>
    public long StudentTelegramId { get; set; }

    /// <summary>Дата и время начала занятия (UTC).</summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>Продолжительность занятия в минутах (по умолчанию 60).</summary>
    public int DurationMinutes { get; set; } = 60;

    /// <summary>Текущий статус записи.</summary>
    public LessonStatus Status { get; set; } = LessonStatus.Pending;

    /// <summary>Дата и время создания записи (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Произвольные заметки к уроку (опционально).</summary>
    public string? Notes  { get; set; }
    
    /// <summary>Навигационное свойство: ученик, записанный на урок.</summary>
    public User? Student { get; set; }
}

/// <summary>
/// Возможные статусы урока.
/// </summary>
public enum LessonStatus
{
    /// <summary>Заявка создана и ожидает подтверждения репетитором.</summary>
    Pending = 0,

    /// <summary>Репетитор подтвердил занятие.</summary>
    Confirmed = 1,

    /// <summary>Репетитор отклонил заявку.</summary>
    Declined = 2,

    /// <summary>Занятие отменено одной из сторон.</summary>
    Canceled = 3,
}