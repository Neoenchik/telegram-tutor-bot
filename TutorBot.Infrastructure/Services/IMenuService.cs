using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Domain.Entities;

namespace TutorBot.Infrastructure.Services;

public interface IMenuService
{
    /// <summary>
    /// Возвращает клавиатуру главного меню в зависимости от роли пользователя.
    /// </summary>
    ReplyKeyboardMarkup GetMainMenuKeyboard(User user);
    
    ReplyKeyboardMarkup GetProfileKeyboard();
    
    InlineKeyboardMarkup GetCalendarKeyboard(DateTime start, int days);
    
    InlineKeyboardMarkup GetTimesKeyboard(DateTime selectedDate);
}