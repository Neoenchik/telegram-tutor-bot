namespace TutorBot.Domain.Entities;

/// <summary>
/// Состояния, в которых может находиться пользователь при взаимодействии с ботом.
/// Используется для маршрутизации последующих вводов пользователя.
/// </summary>
public enum ConversationState
{
    /// <summary>
    /// Нет ожидающих действий, пользователь находится в исходном состоянии.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// Ожидается ввод имени/фамилии для заполнения профиля.
    /// </summary>
    AwaitingProfileName,

    /// <summary>
    /// Ожидается ввод класса/уровня ученика.
    /// </summary>
    AwaitingProfileClass,

    /// <summary>
    /// Ожидается ввод предмета обучения.
    /// </summary>
    AwaitingProfileSubject,

    /// <summary>
    /// Пользователь выбирает дату для записи на занятие.
    /// </summary>
    ChoosingDateForBooking,

    /// <summary>
    /// Пользователь выбирает время для ранее выбранной даты.
    /// </summary>
    ChoosingTimeForBooking,

    /// <summary>
    /// Пользователь подтверждает выбранное время/дату.
    /// </summary>
    ConfirmingBooking,

    // можно дальше добавлять: Rescheduling, AddingRecurringSlot и т.д.
}