using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TutorBot.Webhook.Handlers;

/// <inheritdoc />
public class UpdateHandler: IUpdateHandler
{
    /// <summary>
    /// Логгер
    /// </summary>
    private readonly ILogger<UpdateHandler> _logger;
    private readonly MessageHandler _messageHandler;
    private readonly CallbackQueryHandler _callbackHandler;
    
    public UpdateHandler(ILogger<UpdateHandler> logger, MessageHandler messageHandler, CallbackQueryHandler callbackHandler)
    {
        _logger = logger;
        _messageHandler = messageHandler;
        _callbackHandler = callbackHandler;
    }
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Получено обновление типа {UpdateType}", update.Type);
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message != null)
                        await _messageHandler.HandleAsync(botClient, update.Message, cancellationToken);
                    break;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery != null)
                        await _callbackHandler.HandleAsync(botClient, update.CallbackQuery, cancellationToken);
                    break;

                default:
                    _logger.LogInformation("Не обрабатываемый тип обновления: {UpdateType}", update.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке обновления");
            //можно отправить ошибку админу
        }
    }
}