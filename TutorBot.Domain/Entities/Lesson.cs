namespace TutorBot.Domain.Entities;

public class Lesson
{
    public Guid Id { get; set; } //PK
    public long StudentTelegramId { get; set; } //FK to User.Id
    public DateTime StartDateTime { get; set; } //UTc
    public int DurationMinutes { get; set; } = 60; //60 минут
    public LessonStatus Status { get; set; } = LessonStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes  { get; set; } //Для будущего: комментарии
    
    //Навигация
    public User? Student { get; set; }
}

public enum LessonStatus
{
    Pending = 0,
    Confirmed = 1,
    Declined = 2,
    Canceled = 3,
}