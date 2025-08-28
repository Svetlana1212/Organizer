using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Pdf.Operators;
using BusinessNotes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using static System.Net.Mime.MediaTypeNames;

namespace Organizer
{   
    /// <summary>
    /// Реализует методы для работы с телеграм ботом.
    /// </summary>
    public class TelegramManager
    {
        BusinessNotesManager businessNotesManager = new BusinessNotesManager();

        #region Методы
        /// <summary>
        /// Выводит меню.
        /// </summary>
        /// <param name = "botClient" > TG Bot API клиента.</param>
        /// <param name="update">Тип события.</param>
        /// <param name="cancellationToken">Прерывание запроса.</param>
        public async void MenuOutput(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            var chatId = update.Message.Chat.Id;
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]{
            new[]
                {
                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData($"Заметки на сегодня ({DateTime.Today.ToString("dd MM yyyy")}) ", "listDay")
                },
            new[]
                 {
                     Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Просмотреть все заметки", "listAll")
                 },
            new[]
                 {
                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Создать заметку", "create")
                 },
            new[]
                 {
                     Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Мой календарь", "calendar")
                 },
           /* new[]
                 {
                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Удалить заметку", "delete")
                 },*/
            new[]
                 {
                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Удалить старые заметки", "deleteAll")
                 }
            });
            await botClient.SendMessage(
              chatId: chatId,
              text: $"Выбери действие:\n\n",
              replyMarkup: keyboard,
              cancellationToken: cancellationToken

            );
            return;
        }

        /// <summary>
        /// Формирует список заметок.
        /// </summary>
        /// <param name="botClient">TG Bot API клиента.</param>
        /// <param name="update">Тип события.</param>
        /// <param name="cancellationToken">Прерывание запроса.</param>
        /// <param name="all">Параметр,определяющий нужен ли полный список.</param>
        /// <returns>Возвращает сообщение со списком заметок</returns>
        public async Task NoteAllAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken, bool all = false)
        {
            DateTime date = (UpdateHandler.CurrentStatus == "date")? DateTime.Today : Calendar.CurrentDate;
            var chatId = update.CallbackQuery.Message.Chat.Id;
            List<Note> myNotes;
            string title;
            string text;
            if (!all)
            {
                myNotes = businessNotesManager.ListCreate(date);
                title = $"Заметки на {date.ToString("d MMMM yyyy")}\n";
            }
            else
            {
                myNotes = businessNotesManager.ListCreate(date,all = true);
                title = $"Все заметки\n";
            }
            text = title;
            int i = 1;
            if (myNotes.Count > 0)
            {
                foreach (var note in myNotes)
                {                    
                    if (note.UserId == chatId)
                    {
                        if (all == true)
                        {
                            text += $"{i}) {note.Description}\n дата показа {note.DisplayDate.ToString("dd.MM.yyyy")}  Номер заметки -{note.Id}\n\n";
                            i++;
                        }
                        else
                        {
                            text += $"{i}) {note.Description}\n\n";
                            i++;
                        }
                        
                    }

                }
                text = text + "Для продолжения работы введите команду /menu";
                await botClient.SendMessage(
                chatId: chatId,
                text: text,
                cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.SendMessage(
                chatId: chatId,
                text: $"Нет заметок на {date.ToString("d MMMM yyyy")}.\n Для продолжения работы введите команду /menu",
                cancellationToken: cancellationToken
                );
            }
            
        }
        /// <summary>
        /// Выводит календарь пользователю.
        /// </summary>
        /// <param name="botClient">TG Bot API клиента.</param>
        /// <param name="update">Объект событие.</param>
        /// <returns></returns>
        public async Task GetCalendarAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update,long chatId)
        {
            await Calendar.SendCalendarAsync(botClient, chatId, DateTime.Now);
        }
        

        /// <summary>
        /// Создает новую заметку.
        /// </summary>
        /// <param name="botClient">TG Bot API клиента.</param>
        /// <param name="update">Объект событие.</param>
        /// <param name="chatId">Идентификатор чата.</param>
        /// <param name="cancellationToken">Прерывание запроса.</param>
        /// <returns>Сообщение, что заметка создана </returns>
        public async Task CreateNoteAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, long chatId, CancellationToken cancellationToken)
        {            
            DateTime date;            
            if (UpdateHandler.CurrentStatus == "text")
            {
                await botClient.SendMessage(
                chatId: chatId,
                text: "Введите текст заметки",
                cancellationToken: cancellationToken
                );
                UpdateHandler.CurrentStatus = "date";
            }
            else if (UpdateHandler.CurrentStatus == "newNote")
            {              
                string description = UpdateHandler.CurrentMessage;
                Note newNote = new Note(description, Calendar.CurrentDate);
                newNote.UserId = chatId;
                string finalText;
                if (businessNotesManager.Add(newNote))
                {
                    finalText = $"Вы создали заметку c номером {newNote.Id}.\n Для продолжения работы введите команду /menu";
                }
                else
                {
                    finalText = "Что-то пошло не так. Заметка не создана, попробуйте еще раз.\n Для продолжения работы введите команду /menu";
                }
                await botClient.SendMessage(
                    chatId: chatId,
                    text: finalText,
                    cancellationToken: cancellationToken
                    );
                UpdateHandler.CurrentStatus = string.Empty;
                UpdateHandler.CurrentMessage = string.Empty;
                Calendar.CurrentDate = DateTime.Today;
            }
        }

       /* public async Task DeleteNoteAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            long chatId = update.CallbackQuery.Message.Chat.Id;
            string text=string.Empty;
             if (UpdateHandler.CurrentStatus == "update")
             {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Введите номер заметки",
                    cancellationToken: cancellationToken
                );                
                UpdateHandler.CurrentStatus = "updateId";
             }
             else if(UpdateHandler.CurrentStatus == "updateId")
             {
                if(Int32.TryParse(UpdateHandler.CurrentMessage,out int Id))
                {
                    var updateNote = businessNotesManager.Search(Id);
                    businessNotesManager.Delete(updateNote);
                    text = "Заметка удалена";
                }
                else
                {
                    text = "Неверный ввод, должно быть целое число";
                }

             }
             else
             {
                text = "Что-то пошло не так... Попробуйте снова";
             }
            await botClient.SendMessage(
                   chatId: chatId,
                   text: text,
                   cancellationToken: cancellationToken
            );
            UpdateHandler.CurrentStatus = string.Empty;
            UpdateHandler.CurrentMessage = string.Empty;
        }*/

        /// <summary>
        /// Удаляет старые заметки.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="chatId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteNotesAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, long chatId, CancellationToken cancellationToken)
        {
            bool all;
            DateTime date = DateTime.Today;
            var myNotes = businessNotesManager.ListCreate(date,all=true);
            string userMessage = $"У вас нет заметок\n Для продолжения работы введите команду /menu";
            if (myNotes.Count > 0)
            {
                int count = 0;                
                foreach (var note in myNotes)
                {                    
                    if (note.UserId == chatId)
                    {                        
                        int result = DateTime.Compare(DateTime.Today, note.DisplayDate);
                        if (result>0)
                        {
                            if (businessNotesManager.Delete(note))
                            {
                                count++;
                            }                            
                        }                        
                    }
                    if (count == 0)
                    {
                        userMessage = $"Нет заметок для удаления\n Для продолжения работы введите команду /menu";
                    }
                    else
                    {
                        userMessage = "Заметки удалены \n Для продолжения работы введите команду /menu";
                    }
                }                            
            }            
            await botClient.SendMessage(
                chatId: chatId,
                text: userMessage,
                cancellationToken: cancellationToken
            );
        }

        #endregion
    }
}
