using Microsoft.EntityFrameworkCore;
using TutorBot.Domain.Entities;

namespace TutorBot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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
    }
}