using TutorBot.Domain.Entities;
using TutorBot.Infrastructure.Data;

namespace TutorBot.Infrastructure.Services;

/// <summary>
/// Сервис для управления данными пользователя: создание, обновление состояния, проверка прав.
/// Работает поверх <see cref="AppDbContext"/>.
/// </summary>
public class UserService
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Создаёт экземпляр <see cref="UserService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Возвращает существующего пользователя по Telegram ID или создаёт нового.
    /// </summary>
    /// <param name="telegramId">Telegram‑идентификатор пользователя.</param>
    /// <param name="firstName">Имя из профиля Telegram.</param>
    /// <param name="lastName">Фамилия из профиля Telegram.</param>
    /// <param name="username">Юзернейм из профиля Telegram.</param>
    /// <returns>Сущность <see cref="User"/>, существующая или новая.</returns>
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
    
    /// <summary>
    /// Проверяет, имеет ли пользователь роль администратора.
    /// </summary>
    /// <param name="user">Проверяемый пользователь.</param>
    /// <returns><c>true</c>, если роль <see cref="UserRole.Admin"/>; иначе <c>false</c>.</returns>
    public bool IsAdmin(User user) => user.Role == UserRole.Admin;

    /// <summary>
    /// Обновляет состояние разговора пользователя и дополнительные данные состояния.
    /// </summary>
    /// <param name="telegramId">Telegram‑ID пользователя.</param>
    /// <param name="newState">Новое состояние.</param>
    /// <param name="stateData">Дополнительная строковая информация (может быть <c>null</c>).</param>
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

    /// <summary>
    /// Сбрасывает состояние пользователя к <see cref="ConversationState.Idle"/>.
    /// </summary>
    /// <param name="telegramId">Telegram‑ID пользователя.</param>
    public async Task ClearStateAsync(long telegramId)
    {
        await UpdateStateAsync(telegramId, ConversationState.Idle);
    }
}