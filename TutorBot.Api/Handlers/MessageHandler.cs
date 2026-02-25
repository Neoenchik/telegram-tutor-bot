using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Infrastructure.Services;
using TutorBot.Domain.Entities;
using User = TutorBot.Domain.Entities.User;

namespace TutorBot.Webhook.Handlers;

/// <summary>
/// Обработчик сообщений
/// </summary>
public class MessageHandler
{
    private readonly ILogger<MessageHandler> _logger;
    private readonly UserService _userService;
    private readonly IMenuService _menuService;

    /// <summary>
    /// Создаёт новый экземпляр <see cref="MessageHandler"/>.
    /// </summary>
    /// <param name="logger">Логгер для записи информации и ошибок.</param>
    /// <param name="userService">Сервис для работы с пользователями.</param>
    /// <param name="menuService">Сервис для построения клавиатур.</param>
    public MessageHandler(ILogger<MessageHandler> logger,
                          UserService userService,
                          IMenuService menuService)
    {
        _logger = logger;
        _userService = userService;
        _menuService = menuService;
    }

    /// <summary>
    /// Обрабатывает входящее текстовое сообщение от пользователя и выполняет соответствующее действие.
    /// </summary>
    /// <param name="botClient">Клиент Telegram‑бота.</param>
    /// <param name="message">Полученное сообщение.</param>
    /// <param name="ct">Токен отмены.</param>
    public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        if(message.Text is null) return;

        var text = message.Text.Trim();
        var user = await _userService.GetOrCreateUserAsync(
            message.From!.Id,
            message.From.FirstName,
            message.From.LastName,
            message.From.Username);

        switch (text.ToLowerInvariant())
        {
            case "/start":
                await ShowMainMenuAsync(botClient, message.Chat.Id, user, ct);
                break;
            
            case "📅 записаться на занятие":
                await  StartBookingProcessAsync(botClient, message.Chat.Id, user, ct);
                break;
            
            default:
                await botClient.SendMessage(
                    message.Chat.Id,
                    $"Получено: {text}\n\nИспользуй меню ниже ↓",
                    replyMarkup: _menuService.GetMainMenuKeyboard(user),
                    cancellationToken: ct);
                break;
        }
    }

    private async Task ShowMainMenuAsync(ITelegramBotClient botClient, long chatId, User user, CancellationToken ct)
    {
        string welcome = user.Role == UserRole.Admin
            ? "Добро пожаловать, администратор"
            : "Привет! Я помогу записаться на занятия.";

        await botClient.SendMessage(
            chatId: chatId,
            text: welcome + "\n\nВыбери действие:",
            replyMarkup: _menuService.GetMainMenuKeyboard(user),
            cancellationToken: ct);
    }

    private async Task StartBookingProcessAsync(ITelegramBotClient bot, long chatId, User user,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(user.DisplayName))
        {
            await bot.SendMessage(
                chatId: chatId,
                text: "Сначала заполните профиль, чтобы репетитор знал, с кем занимается",
                replyMarkup: _menuService.GetProfileKeyboard(),
                cancellationToken: ct);

            await _userService.UpdateStateAsync(user.Id, ConversationState.AwaitingProfileName);
            return;
        }
        
        await _userService.UpdateStateAsync(user.Id, ConversationState.ChoosingDateForBooking);

        var calendar = _menuService.GetCalendarKeyboard(DateTime.UtcNow.Date, 30);
        
        await bot.SendMessage(
            chatId: chatId,
            "Выберите дату занятия (ближайшие 30 дней):",
            replyMarkup: calendar,
            cancellationToken: ct);
    }
}