using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using TutorBot.Infrastructure.Data;
using TutorBot.Infrastructure.Services;
using TutorBot.Webhook.Handlers;

var builder = WebApplication.CreateBuilder(args);


// logging
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

//Telegram
var token = builder.Configuration["Telegram:BotToken"]
    ?? throw new Exception("Telegram bot token not found");
var botClient = new TelegramBotClient(token);
builder.Services.AddSingleton<ITelegramBotClient>(botClient);

builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<MessageHandler>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CallbackQueryHandler>();
builder.Services.AddScoped<LessonService>();

//Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

//Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// automatically apply any pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

//Middleware
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapHealthChecks("/health");

var webhookBase = builder.Configuration["Telegram:WebhookBaseUrl"];
if(string.IsNullOrWhiteSpace(webhookBase))
    throw new Exception("Webhook base url not found");

var fullWebhookUrl = $"{webhookBase.TrimEnd('/')}/webhook";

try
{
    var currentWebhook = await botClient.GetWebhookInfo();
    if (currentWebhook.Url != fullWebhookUrl)
    {
        await botClient.SetWebhook(
            url: fullWebhookUrl,
            // allowedUpdates: new[] { UpdateType.Message, UpdateType.CallbackQuery }, // можно ограничить
            dropPendingUpdates: true  // очень полезно при перезапуске
        );

        Console.WriteLine($"Webhook успешно установлен: {fullWebhookUrl}");
    }
    else
    {
        Console.WriteLine($"Webhook уже установлен корректно: {fullWebhookUrl}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка установки webhook: {ex.Message}");
}

// handler
app.MapPost("/webhook", async (HttpContext ctx, IUpdateHandler handler, ITelegramBotClient bot) =>
{
    var update = await ctx.Request.ReadFromJsonAsync<Update>(cancellationToken: ctx.RequestAborted);
    
    if(update is null)
        return Results.BadRequest();
    
    await handler.HandleUpdateAsync(botClient, update, ctx.RequestAborted);
    return Results.Ok();
});

app.Run();
