using Microsoft.EntityFrameworkCore;
using TutorBot.Domain.Entities;
using TutorBot.Infrastructure.Data;

namespace TutorBot.Infrastructure.Services;

public class LessonService
{
    private readonly AppDbContext _db;

    public LessonService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Lesson> CreatePendingLessonAsync(long studentId, DateTime startUtc)
    {
        var lesson = new Lesson
        {
            StudentTelegramId =  studentId,
            StartDateTime =  startUtc,
        };
        
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();
        return lesson;
    }
    
    public async Task UpdateStatusAsync(Guid lessonId, LessonStatus newStatus)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if (lesson == null)
            return;
        
        lesson.Status = newStatus;
        await _db.SaveChangesAsync();
    }

    public async Task<Lesson?> GetLessonByIdAsync(Guid lessonId)
    {
        return await _db.Lessons
            .Include(l=>l.Student)
            .FirstOrDefaultAsync(l => l.Id == lessonId);
    }
}