namespace TutorBot.Domain.Entities;

/// <summary>
/// Представляет пользователя бота, сохранённого в базе данных.
/// Хранит идентификатор Telegram, информацию профиля и текущее состояние взаимодействия.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя (Telegram ID).
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Имя пользователя в Telegram (@username). Может быть <c>null</c>.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Имя, указанное в профиле Telegram.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия, указанная в профиле Telegram.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Роль пользователя в системе (ученик, администратор).
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Student; 

    /// <summary>
    /// Время создания записи (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;

    /// <summary>
    /// Время последней активности пользователя (UTC).
    /// </summary>
    public DateTime LastActivity { get; set; }
    
    /// <summary>
    /// Отображаемое имя (например, полное имя ученика).
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Текущее состояние общения с ботом (машина состояний).
    /// </summary>
    public ConversationState State { get; set; } = ConversationState.Idle;

    /// <summary>
    /// Дополнительные данные состояния (JSON, выбранная дата и т.п.).
    /// </summary>
    public string? StateData { get; set; }
    
    /// <summary>
    /// Идентификатор часового пояса пользователя, используется для конвертации времени.
    /// </summary>
    public string TimeZoneId { get; set; } = "Europe/Moscow";
}

/// <summary>
/// Роли, которые может иметь пользователь в системе.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Обычный ученик, не обладает правами администратора.
    /// </summary>
    Student = 0,

    /// <summary>
    /// Администратор, имеет дополнительные возможности управления.
    /// </summary>
    Admin = 1
};