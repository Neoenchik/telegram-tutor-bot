namespace TutorBot.Domain.Entities;

public enum ConversationState
{
    Idle = 0,                    // обычное состояние, ничего не ждём
    AwaitingProfileName,         // просим имя/фамилию
    AwaitingProfileClass,        // класс/уровень
    AwaitingProfileSubject,      // предмет
    ChoosingDateForBooking,      // выбор даты записи
    ChoosingTimeForBooking,      // выбор времени
    ConfirmingBooking,           // подтверждение записи
    // можно дальше добавлять: Rescheduling, AddingRecurringSlot и т.д.
}