using Microsoft.EntityFrameworkCore;
using TutorBot.Domain.Entities;
using TutorBot.Infrastructure.Data;

namespace TutorBot.Infrastructure.Services;

/// <summary>
/// Сервис для управления уроками: создание, обновление статуса и получение доступных слотов.
/// Работает поверх <see cref="AppDbContext"/>.
/// </summary>
public class LessonService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="LessonService"/>.
    /// </summary>
    /// <param name="db">Контекст базы данных.</param>
    public LessonService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Создаёт новый урок со статусом <c>Pending</c>, предварительно проверяя конфликты.
    /// </summary>
    /// <param name="studentId">Telegram‑ID ученика.</param>
    /// <param name="startUtc">Дата и время начала занятия (UTC).</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>
    /// Кортеж, где <c>Success</c> указывает на отсутствие конфликта,
    /// <c>Error</c> — текст ошибки (если есть), <c>lesson</c> — созданный объект.
    /// </returns>
    public async Task<(bool Success, string? Error,Lesson? lesson)> CreatePendingLessonAsync(long studentId, DateTime startUtc, CancellationToken ct = default)
    {
        //првоерка, нет ли уже подтвержденного занятия
        var conflict = await  _db.Lessons.AnyAsync(l=> 
            (l.StartDateTime == startUtc 
            && l.Status == LessonStatus.Pending),
            cancellationToken: ct);

        if (conflict)
        {
            return (false, "Этот слот уже занят и подтвержден", null);
        }
        
        var lesson = new Lesson
        {
            StudentTelegramId =  studentId,
            StartDateTime =  startUtc,
        };
        
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync(ct);
        return (true, null, lesson);
    }
    
    /// <summary>
    /// Обновляет статус урока по его идентификатору.
    /// </summary>
    /// <param name="lessonId">Идентификатор урока.</param>
    /// <param name="newStatus">Новый статус.</param>
    public async Task UpdateStatusAsync(Guid lessonId, LessonStatus newStatus)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if (lesson == null)
            return;
        
        lesson.Status = newStatus;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Возвращает урок по идентификатору, включая данные ученика.
    /// </summary>
    /// <param name="lessonId">Идентификатор урока.</param>
    /// <returns>Объект <see cref="Lesson"/> или <c>null</c>, если не найден.</returns>
    public async Task<Lesson?> GetLessonByIdAsync(Guid lessonId)
    {
        return await _db.Lessons
            .Include(l=>l.Student)
            .FirstOrDefaultAsync(l => l.Id == lessonId);
    }

    /// <summary>
    /// Возвращает список доступных временных слотов (UTC) для заданной даты.
    /// Учитывает повторяющиеся слоты, исключения и уже подтверждённые уроки.
    /// </summary>
    /// <param name="date">Дата, для которой требуются свободные слоты.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список <see cref="DateTime"/> (UTC) доступных слотов, упорядоченных по возрастанию.</returns>
    public async Task<List<DateTime>> GetAvailableSlotsForDateAsync(DateOnly date, CancellationToken ct)
    {
        var utcNow = DateTime.UtcNow;
        var targetDate = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        
        var weekday = (int)date.DayOfWeek;
        var weekdayAdjusted = weekday == 0 ? 6 : weekday - 1;

        var reccuring = await _db.RecurringSlots
            .Where(r => r.Weekday == weekdayAdjusted)
            .ToListAsync(ct);

        if (!reccuring.Any())
            return new List<DateTime>();

        var exceptions = await _db.SlotExceptions
            .Where(e => e.Date == date)
            .ToListAsync(ct);

        var blockedIntervals = new List<(DateTime start, DateTime end)>();

        foreach (var ex in exceptions)
        {
            if (ex.FullDay)
                return new List<DateTime>();

            if (ex.UntilTime.HasValue)
            {
                var blockEnd = targetDate.Date + ex.UntilTime.Value;
                blockedIntervals.Add((targetDate.Date, blockEnd));
            }
        }

        var possibleSlots = new List<DateTime>();

        foreach (var slot in reccuring)
        {
            var slotStart = targetDate.Date + slot.StartTime;
            var slotEnd = slotStart + TimeSpan.FromMinutes(slot.DurationMinutes);

            bool isBlocked = blockedIntervals.Any(b =>
                slotStart < b.end && slotEnd > b.start);
            
            if (!isBlocked)
                possibleSlots.Add(slotStart);
        }

        var booked =await _db.Lessons
            .Where(l => l.Status == LessonStatus.Confirmed
                        && l.StartDateTime.Date == targetDate.Date)
            .Select(l => l.StartDateTime)
            .ToListAsync(ct);

        var available = possibleSlots
            .Except(booked)
            .Where(s => s > utcNow.AddMinutes(30))
            .OrderBy(s => s)
            .ToList();
        
        return available;
    }
}