using Microsoft.EntityFrameworkCore;
using TutorBot.Domain.Entities;

namespace TutorBot.Infrastructure.Data;

/// <summary>
/// Entity Framework контекст базы данных TutorBot.
/// Содержит коллекции всех доменных сущностей и настройку модели.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Коллекция пользователей бота.</summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>Коллекция уроков (записей на занятие).</summary>
    public DbSet<Lesson> Lessons { get; set; } = null!;

    /// <summary>Коллекция повторяющихся слотов расписания репетитора.</summary>
    public DbSet<RecurringSlot> RecurringSlots { get; set; } = null!;

    /// <summary>Коллекция исключений (закрытых дней/интервалов) в расписании.</summary>
    public DbSet<SlotException> SlotExceptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();
            entity.Property(u => u.Role).HasConversion<int>();
            entity.Property(u => u.State).HasConversion<int>();
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Status).HasConversion<int>();
            
            entity.HasOne(l => l.Student)
                .WithMany()
                .HasForeignKey(l=>l.StudentTelegramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecurringSlot>(entity =>
        {
            entity.HasKey(r => r.Id);
        });
        
        modelBuilder.Entity<SlotException>(entity =>
        {
            entity.HasKey(s => s.Id);
        });
    }
}