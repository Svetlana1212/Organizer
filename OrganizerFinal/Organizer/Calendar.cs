using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.Collections.Generic;

/// <summary>
/// Календарь.
/// </summary>
public class Calendar
{
    #region Поля и свойства
    /// <summary>
    /// Записывает выбранную пользователем дату.
    /// </summary>
    public static DateTime CurrentDate = DateTime.Today;
    #endregion

    #region Методы
    /// <summary>
    /// Отрисовывает каледарь.
    /// </summary>
    /// <param name="date">Текущая дата.</param>
    /// <returns>Макет календаря на текущий месяц.</returns>
    public static InlineKeyboardMarkup GenerateCalendar(DateTime date)
    {        
        DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        if (dayOfWeek == 0) dayOfWeek = 7;         
        List<InlineKeyboardButton[]> keyboardRows = new List<InlineKeyboardButton[]>();        
        keyboardRows.Add(new InlineKeyboardButton[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" });
        List<InlineKeyboardButton> firstWeek = new List<InlineKeyboardButton>();
        for (int i = 1; i < dayOfWeek; i++)
        {
            firstWeek.Add(InlineKeyboardButton.WithCallbackData(" ", "empty"));
        }        
        for (int day = 1; day <= daysInMonth; day++)
        {
            firstWeek.Add(InlineKeyboardButton.WithCallbackData(day.ToString(), $"day_{day}.{date.ToString("MM")}.{date.Year}")); // CallbackData содержит информацию о выбранной дате
            if (firstWeek.Count == 7)
            {
                keyboardRows.Add(firstWeek.ToArray());
                firstWeek = new List<InlineKeyboardButton>();
            }
        }       
        if (firstWeek.Count > 0)
        {
            while (firstWeek.Count < 7)
            {
                firstWeek.Add(InlineKeyboardButton.WithCallbackData(" ", "empty"));
            }
            keyboardRows.Add(firstWeek.ToArray());
        }        
        keyboardRows.Add(new InlineKeyboardButton[] {
            InlineKeyboardButton.WithCallbackData("<", $"prev_{date.Year}-{date.Month}"),
            InlineKeyboardButton.WithCallbackData($"{date:MMMM yyyy}", "ignore"), 
            InlineKeyboardButton.WithCallbackData(">", $"next_{date.Year}-{date.Month}")
        });

        return new InlineKeyboardMarkup(keyboardRows.ToArray());
    }

    /// <summary>
    /// Обработывает нажатие кнопок.
    /// </summary>
    /// <param name="botClient">TG Bot API клиента.</param>
    /// <param name="callbackQuery">Возвращаемые данные.</param>
    public async void HandleCalendarCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        string callbackData = callbackQuery.Data;
        if (callbackData.StartsWith("day"))
        {            
            string selectedDate = callbackData.Substring(4);            
            CurrentDate = DateTime.Parse(selectedDate);
            await botClient.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,replyMarkup: null);            
        }
        else if (callbackData.StartsWith("next") || callbackData.StartsWith("prev"))
        {
            
            string[] parts = callbackData.Split('_');
            string[] dateParts = parts[1].Split('-');
            int year = int.Parse(dateParts[0]);
            int month = int.Parse(dateParts[1]);

            DateTime newDate;
            if (parts[0] == "next")
            {
                newDate = new DateTime(year, month, 1).AddMonths(1);
            }
            else
            {
                newDate = new DateTime(year, month, 1).AddMonths(-1);
            }

            InlineKeyboardMarkup newCalendar = GenerateCalendar(newDate);
            await botClient.EditMessageReplyMarkup(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: newCalendar);
        }
        else
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id);
        }
    }

    /// <summary>
    /// Отправляет календарь пользователю.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="initialDate">Начальная дата.</param>
    /// <param name="messageText">Сообщение перед календарем.</param>
    /// <returns>Календарь.</returns>
    public static async Task SendCalendarAsync(ITelegramBotClient botClient, long chatId, DateTime initialDate, string messageText = "Выберите дату:")
    {
        
        InlineKeyboardMarkup calendarKeyboard = GenerateCalendar(initialDate);
        await botClient.SendMessage(
            chatId: chatId,
            text: messageText,
            replyMarkup: calendarKeyboard
        );
    }
    #endregion
}

