using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Domain.Entities;

namespace TutorBot.Infrastructure.Services;

public class MenuService : IMenuService
{
    public ReplyKeyboardMarkup GetMainMenuKeyboard(User user)
    {
        var buttons = new List<KeyboardButton[]>();

        if (user.Role == UserRole.Admin)
        {
            buttons.Add([new KeyboardButton("🗓 Управление расписанием")]);
            buttons.Add([
                new KeyboardButton("📋 Заявки"),
                new KeyboardButton("👥 Ученики")
            ]);
            buttons.Add([new KeyboardButton("➕ Добавить слот")]);
        }
        else
        {
            buttons.Add([new KeyboardButton("📅 Записаться на занятие")]);
            buttons.Add([
                new KeyboardButton("📚 Мои записи"),
                new KeyboardButton("👤 Мой профиль")
            ]);
        }

        buttons.Add([new KeyboardButton("❓ Помощь")]);

        return new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };
    }

    public ReplyKeyboardMarkup GetProfileKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { new KeyboardButton("Заполнить профиль") }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
    }

    public InlineKeyboardMarkup GetCalendarKeyboard(DateTime start, int days)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        var currentRow = new List<InlineKeyboardButton>();

        for (int i = 0; i < days; i++)
        {
            var date = start.AddDays(i);
            var text = date.ToString("dd MMM");
            
            currentRow.Add(InlineKeyboardButton.WithCallbackData(text, $"date:{date:yyyy-MM-dd}"));

            if (currentRow.Count == 3 || i == days - 1) //по 3 кнопки в ряд
            {
                buttons.Add(currentRow.ToArray());
                currentRow.Clear();
            }
        }
        
        if(currentRow.Count>0)
            buttons.Add(currentRow.ToArray());
        
        buttons.Add([
            InlineKeyboardButton.WithCallbackData("❌ Отмена", "cancel")
        ]);

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetTimesKeyboard(DateTime selectedDate)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        var row = new List<InlineKeyboardButton>();

        for (int t = 8; t < 24; t++)
        {
            var callback = $"time:{selectedDate:yyyy-MM-dd}_{t}:00";
            row.Add(InlineKeyboardButton.WithCallbackData($"{t}:00", callback));
            
            if (row.Count == 3)
            {
                buttons.Add(row.ToArray());
                row.Clear();
            }
        }
        
        if(row.Count>0)
            buttons.Add(row.ToArray());
        
        buttons.Add([
            InlineKeyboardButton.WithCallbackData("Своё время", "time:custom"),
            InlineKeyboardButton.WithCallbackData("❌ Отмена", "cancel")
        ]);

        return new InlineKeyboardMarkup(buttons);
    }
}