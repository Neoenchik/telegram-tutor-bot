namespace TutorBot.Domain.Entities;

public class User
{
    /// <summary>
    /// Telegram ID
    /// </summary>
    public long Id { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    /// <summary>
    /// "student", "admin"
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Student; 
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    public DateTime LastActivity { get; set; }
    
    //Для будущего: имя ученика, класс, предмет и т.д.
    public string? DisplayName { get; set; }
    
    //машина состояний
    public ConversationState State { get; set; } = ConversationState.Idle;
    public string? StateData { get; set; } // JSON или строка с контекстом (дата, lessonId и т.д.)
}

public enum UserRole
{
    Student = 0,
    Admin = 1
};