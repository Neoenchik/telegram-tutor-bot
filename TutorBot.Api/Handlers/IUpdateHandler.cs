using Telegram.Bot;
using Telegram.Bot.Types;

namespace TutorBot.Webhook.Handlers;
/// <summary>
/// Базовый обработчик обновлений
/// </summary>
/// <summary>
/// Определяет контракт обработчика Telegram-обновлений.
/// </summary>
public interface IUpdateHandler
{
    /// <summary>
    /// Обрабатывает произвольное <see cref="Update"/>, полученное от Telegram.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram API.</param>
    /// <param name="update">Данные обновления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}