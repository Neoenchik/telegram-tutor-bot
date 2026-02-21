using TutorBot.Domain.Entities;
using TutorBot.Infrastructure.Data;

namespace TutorBot.Infrastructure.Services;

public class UserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> GetOrCreateUserAsync(long telegramId, string? firstName, string? lastName, string? username)
    {
        var user = await _dbContext.Users.FindAsync(telegramId);

        if (user is not null)
        {
            user.LastActivity = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return user;
        }

        user = new User()
        {
            Id = telegramId,
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            DisplayName = $"{firstName} {lastName}".Trim(),
            LastActivity = DateTime.UtcNow
        };
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }
    
    public bool IsAdmin(User user) => user.Role == UserRole.Admin;

    public async Task UpdateStateAsync(long telegramId, ConversationState newState, string? stateData = null)
    {
        var user = await _dbContext.Users.FindAsync(telegramId);
        if (user is null)
            return;
        
        user.State = newState;
        user.StateData = stateData;
        user.LastActivity = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task ClearStateAsync(long telegramId)
    {
        await UpdateStateAsync(telegramId, ConversationState.Idle);
    }
}