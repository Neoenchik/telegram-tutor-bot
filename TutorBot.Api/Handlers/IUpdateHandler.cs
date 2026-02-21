using Telegram.Bot;
using Telegram.Bot.Types;

namespace TutorBot.Webhook.Handlers;
/// <summary>
/// Базовый обработчик обновлений
/// </summary>
public interface IUpdateHandler
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}