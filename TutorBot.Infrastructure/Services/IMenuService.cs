using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Domain.Entities;

namespace TutorBot.Infrastructure.Services;

/// <summary>
/// Предоставляет методы для построения клавиатур Telegram-бота (меню и кнопок).
/// Реализуется сервисом, который формирует разметку в зависимости от состояния пользователя.
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Возвращает клавиатуру главного меню в зависимости от роли пользователя.
    /// </summary>
    /// <param name="user">Текущий пользователь, чья роль определяет содержимое меню.</param>
    ReplyKeyboardMarkup GetMainMenuKeyboard(User user);
    
    /// <summary>
    /// Возвращает клавиатуру для заполнения профиля (одна кнопка).
    /// </summary>
    ReplyKeyboardMarkup GetProfileKeyboard();
    
    /// <summary>
    /// Строит календарь с кнопками на указанный диапазон дней.
    /// </summary>
    /// <param name="start">Дата начала календаря (UTC).</param>
    /// <param name="days">Количество дней, отображаемых в календаре.</param>
    InlineKeyboardMarkup GetCalendarKeyboard(DateTime start, int days);
    
    /// <summary>
    /// Возвращает клавиатуру с временами для выбранной даты.
    /// </summary>
    /// <param name="selectedDate">Дата, для которой подбираются временные слоты.</param>
    InlineKeyboardMarkup GetTimesKeyboard(DateTime selectedDate);
}