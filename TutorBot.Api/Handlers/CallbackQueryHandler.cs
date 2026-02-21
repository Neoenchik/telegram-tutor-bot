using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Domain.Entities;
using TutorBot.Infrastructure.Services;

namespace TutorBot.Webhook.Handlers;

public class CallbackQueryHandler
{
    private readonly ILogger<CallbackQueryHandler> _logger;
    private readonly UserService _userService;
    private readonly IMenuService _menuService;
    private readonly LessonService _lessonService;
    private IConfiguration _config;

    public CallbackQueryHandler(ILogger<CallbackQueryHandler> logger,
                                UserService userService,
                                IMenuService menuService, LessonService lessonService, IConfiguration config)
    {
        _logger = logger;
        _userService = userService;
        _menuService = menuService;
        _lessonService = lessonService;
        _config = config;
    }

    public async Task HandleAsync(ITelegramBotClient bot, CallbackQuery query, CancellationToken ct)
    {
        if (query.Data is null)
            return;

        var user = await _userService.GetOrCreateUserAsync(
            query.From.Id,
            query.From.FirstName,
            query.From.LastName,
            query.From.Username);
        
        //пример обработки
        if (query.Data == "confirm_booking")
        {
            if (user.State != ConversationState.ConfirmingBooking
                || string.IsNullOrEmpty(user.StateData))
            {
                await bot.AnswerCallbackQuery(query.Id, "Ошибка состояния!", true, cancellationToken: ct);
                return;
            }
            
            var selectedDt =  DateTime.Parse(user.StateData);
            
            var lesson = _lessonService.CreatePendingLessonAsync(user.Id, selectedDt);
            await _userService.ClearStateAsync(user.Id);
            
            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Заявка отправлена репетитору. Ждите подтверждения",
                replyMarkup: _menuService.GetMainMenuKeyboard(user), // возвращаем в главное меню
                cancellationToken: ct);
            
            var adminId = long.Parse(_config["Telegram:AdminId"] ?? "0");
            if (adminId != 0)
            {
                var adminKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅ Подтвердить", $"admin_confirm:{lesson.Id}"),
                        InlineKeyboardButton.WithCallbackData("❌ Отклонить", $"admin_decline:{lesson.Id}"),
                        InlineKeyboardButton.WithCallbackData("✏️ Предложить другое", $"admin_propose:{lesson.Id}")
                    }
                });
                
                await bot.SendMessage(
                    adminId,
                    $"Новая заявка от {user.DisplayName ?? user.FirstName}:\n" +
                    $"Дата/время: {selectedDt:dd MMMM yyyy HH:mm} (UTC)\n" +
                    $"Ученик ID: {user.Id}",
                    replyMarkup: adminKeyboard,
                    cancellationToken: ct);
            }
            
            await bot.AnswerCallbackQuery(query.Id, "Заявка создана", cancellationToken: ct);
        }
        else if (query.Data.StartsWith("admin_confirm:"))
        {
            var lessonId = Guid.Parse(query.Data["admin_confirm".Length..]);
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson is null || !_userService.IsAdmin(user))
            {
                await bot.AnswerCallbackQuery(query.Id, "Доступ запрещен или заявка не найдена", true, cancellationToken: ct);
                return;
            }
            
            await _lessonService.UpdateStatusAsync(lesson.Id, LessonStatus.Confirmed);
            
            await bot.SendMessage(
                lesson.StudentTelegramId,
                $"Ваша запись подтверждена!\nДата/время: {lesson.StartDateTime:dd MMMM yyyy HH:mm}",
                cancellationToken: ct);

            await bot.AnswerCallbackQuery(query.Id, "Подтверждено",cancellationToken: ct);
            
            await bot.EditMessageReplyMarkup(
                query.Message!.Chat.Id,
                query.Message.MessageId,
                replyMarkup: null,
                cancellationToken: ct);
        }
        else if (query.Data.StartsWith("admin_decline:"))
        {
            var lessonId = Guid.Parse(query.Data["admin_confirm".Length..]);
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson is null || !_userService.IsAdmin(user))
            {
                await bot.AnswerCallbackQuery(query.Id, "Доступ запрещен или заявка не найдена", true, cancellationToken: ct);
                return;
            }
            
            await _lessonService.UpdateStatusAsync(lesson.Id, LessonStatus.Declined);
            await bot.SendMessage(
                lesson.StudentTelegramId,
                "К сожалению, ваша запись отклонена репетитором.",
                cancellationToken: ct);
            
            await bot.AnswerCallbackQuery(query.Id, "Отклонено",cancellationToken: ct);
            await bot.EditMessageReplyMarkup(
                query.Message!.Chat.Id,
                query.Message.MessageId,
                replyMarkup: null,
                cancellationToken: ct);
        }
        else if (query.Data.StartsWith("admin_propose:"))
        {
            // Заглушка для предложения другого времени
            await bot.AnswerCallbackQuery(query.Id, "Функция предложения другого времени в разработке", cancellationToken: ct);
        }
        else if (query.Data.StartsWith("date:"))
        {
            string dateStr =  query.Data["date:".Length..];

            if (!DateTime.TryParse(dateStr, out var selectedDate))
            {
                await bot.AnswerCallbackQuery(query.Id, "Неверная дата",true, cancellationToken: ct);
                return;
            }
            //переводим в следующее состояние
            await _userService.UpdateStateAsync(user.Id, ConversationState.ChoosingTimeForBooking, dateStr);
            
            var timesKeyboard = _menuService.GetTimesKeyboard(selectedDate);
            
            await bot.SendMessage(
                query.Message!.Chat.Id,
                $"Выбрана дата: {selectedDate: dd MMMM yyyy}\n\nТеперь выберите время:",
                replyMarkup: timesKeyboard,
                cancellationToken: ct);
            
            await bot.AnswerCallbackQuery(query.Id, "Дата выбрана", cancellationToken: ct);
        }
        else if (query.Data.StartsWith("time:"))
        {
            var timeStr = query.Data["time:".Length..].Split('_');
            if (timeStr.Length != 2 || !DateTime.TryParse(timeStr[0], out var selectedDate)
                || !TimeSpan.TryParse(timeStr[1], out var time))
            {
                await bot.AnswerCallbackQuery(query.Id, "Ошибка формата времени",true, cancellationToken: ct);
                return;
            }
            var selectedDateTime = selectedDate.Add(time);
            await _userService.UpdateStateAsync(user.Id, ConversationState.ConfirmingBooking, selectedDateTime.ToString("O"));
            
            await bot.SendMessage(
                query.Message!.Chat.Id,
                $"Вы хотите записаться на:\n{selectedDate: dd MMMM yyyy в HH:mm}\n\nПодтвердить?",
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("✅ Да, записаться", "confirm_booking"), 
                    InlineKeyboardButton.WithCallbackData("❌ Отмена", "cancel")
                    ),
                cancellationToken: ct);
            
            await bot.AnswerCallbackQuery(query.Id, "Время выбрано", cancellationToken: ct);
        }
        else if (query.Data == "cancel")
        {
            await _userService.ClearStateAsync(user.Id);
            await bot.AnswerCallbackQuery(query.Id, "Действие отменено", cancellationToken: ct);
            await bot.SendMessage(
                query.Message!.Chat.Id,
                "Отменено. Возвращаемся в главное меню.",
                replyMarkup: _menuService.GetMainMenuKeyboard(user),
                cancellationToken: ct);
        }
        else
        {
            await bot.AnswerCallbackQuery(query.Id, "Неизвестная команда", showAlert: true, cancellationToken: ct);
        }

        if (query.Message is { } msg)
        {
            await bot.EditMessageReplyMarkup(
                msg.Chat.Id,
                msg.MessageId,
                replyMarkup: null,
                cancellationToken: ct);
        }
    }
}