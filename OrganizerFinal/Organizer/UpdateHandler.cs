using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;
using Telegram.Bot.Types.ReplyMarkups;
using BusinessNotes;
//using Aspose.Pdf;
using Telegram.BotAPI.AvailableTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Aspose.Pdf.AI;
using Telegram.Bot.Exceptions;


namespace Organizer
{
    /// <summary>
    /// Обработчик входящих обновлений от Telegram.
    /// </summary>
    internal class UpdateHandler : IUpdateHandler
    {
        #region Поля и свойства
        /// <summary>
        /// Создание клиента для работы с Телеграм ботом.
        /// </summary>
        private readonly ITelegramBotClient _botClient;

        /// <summary>
        /// Настройка сериализации JSON.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };


        /// <summary>
        /// Записывает текущий статус пользователя.
        /// </summary>
        public static Dictionary<string, string> CurrentStatus=new Dictionary<string, string>();

        /// <summary>
        /// Записывает текущее сообщение пользователя.
        /// </summary>
        public static Dictionary<string, string> CurrentMessage = new Dictionary<string, string>();  
        
        TelegramManager telegramManager = new TelegramManager();
        Calendar calendar = new Calendar();

        #endregion

        #region Методы
        /// <summary>
        /// Отправка текстового сообщения.
        /// </summary>
        /// <param name="chatId">Id пользователя.</param>
        /// <param name="text">Техт пользователя.</param>
        /// <param name="cancellationToken">Токен для отмены операции.</param>
        private async Task SendTextMessageAsync(long chatId, string text, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(chatId: chatId, text: text, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Логирование входящих обновлений от Telegram.
        /// </summary>
        /// <param name="update">Обновления от Телеграм.</param>
        private void LogUpdate(Update update)
        {
            try
            {
                var json = JsonSerializer.Serialize(update, JsonOptions);                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сериализации: {ex.Message}");
            }
        }
        #endregion

        #region <IUpdateHandler>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            LogUpdate(update);            
            try
            {
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        {
                            if (update.Message is not Telegram.Bot.Types.Message message) return;
                            var chatId = message.Chat.Id;
                            var text = message.Text;
                            var user = message.From;
                            
                            if (message.Type == MessageType.Text && !string.IsNullOrEmpty(text))
                            {
                                if (text == BotChatCommands.Start)
                                {
                                    await SendTextMessageAsync(chatId, $"🙌🏿 Добро пожаловать,{user.FirstName}\n\n" +
                                      "Я бот для работы с твоими заметками 😉\n" +
                                      "Благодаря мне ты не забудешь про свои\n важные дела, встречи и звонки.\n" +
                                      "Ты можешь:\n" +
                                      " * Создавать заметки\n" +
                                      " * Установливать дату для их просмотра\n" +
                                      " * Просматривать заметки на любую дату\n", cancellationToken);
                                    telegramManager.MenuOutput(botClient,update, cancellationToken);
                                }                                
                                else if (text == BotChatCommands.Menu)
                                {
                                    telegramManager.MenuOutput(botClient, update, cancellationToken);
                                }                                
                                else if (CurrentStatus.ContainsKey(chatId.ToString()))                                   
                                {
                                    Console.WriteLine("new status");
                                    Console.WriteLine(CurrentStatus[chatId.ToString()]);
                                    if (CurrentStatus[chatId.ToString()] == "text")
                                    {                                        
                                        CurrentMessage[chatId.ToString()] = message.Text;                                       
                                    }
                                    else if((CurrentStatus[chatId.ToString()] == "date")) //&& (CurrentMessage.ContainsKey(chatId.ToString())))
                                    {
                                        /*if (CurrentMessage[chatId.ToString()] != text)
                                        {*/
                                            Console.WriteLine("Hello date");
                                            telegramManager.GetCalendarAsync(botClient, update, chatId);
                                            telegramManager.CreateNoteAsync(botClient, update, chatId, cancellationToken);
                                            CurrentMessage[chatId.ToString()] = message.Text;
                                            Console.WriteLine(CurrentMessage[chatId.ToString()]);
                                        //}
                                        
                                    }
                                    else if ((CurrentStatus[chatId.ToString()] == "newNote") && (message.Text == BotChatCommands.Ok))
                                    {
                                        telegramManager.CreateNoteAsync(botClient, update, chatId, cancellationToken);
                                    }
                                }                                                                                        
                                else
                                {
                                    await botClient.SendMessage(
                                      chatId: chatId,
                                      text: $"Для продолжения работы нажми:/menu\n" +
                                      $"{BotChatCommands.Menu} - это вывод всех доступных команд\n"

                                    );
                                }
                            }
                        }
                        return;

                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        {
                            var callbackQuery = update.CallbackQuery;
                            var user = callbackQuery.From;
                            var chat = callbackQuery.Message.Chat;
                            string buttonComand = callbackQuery.Data;
                            var parse = buttonComand.Split('_');
                            var action = parse[0];                            

                            switch (action)
                            {
                                case "listDay":
                                    Calendar.CurrentDate[chat.Id.ToString()] = DateTime.Today;
                                    telegramManager.NoteAllAsync(botClient,update, cancellationToken);
                                    break;

                                case "listAll":                                    
                                    bool all = true;
                                    Calendar.CurrentDate[chat.Id.ToString()] = DateTime.Today;
                                    telegramManager.NoteAllAsync(botClient, update,cancellationToken, all);
                                    break;

                                case "create":
                                    CurrentStatus[chat.Id.ToString()]="text";
                                    Console.WriteLine(CurrentStatus[chat.Id.ToString()]);
                                    telegramManager.CreateNoteAsync(botClient, update,chat.Id, cancellationToken);
                                    break;

                                case "deleteAll":                                                                    
                                    telegramManager.DeleteNotesAsync(botClient, update, chat.Id, cancellationToken);
                                    break;                              

                                case "day":
                                    Console.WriteLine(CurrentStatus[chat.Id.ToString()]);
                                    calendar.HandleCalendarCallback(botClient, callbackQuery);
                                    if (CurrentStatus[chat.Id.ToString()] == "date")
                                    {                                        
                                        CurrentStatus[chat.Id.ToString()]="newNote";
                                        await botClient.SendMessage(
                                        chatId: chat.Id,
                                        text: $"Вы выбрали дату {Calendar.CurrentDate[chat.Id.ToString()].ToString("dd MM yyyy")}\n Введите команду /ok"
                                        );
                                    }                                   
                                    else
                                    {
                                        telegramManager.NoteAllAsync(botClient, update, cancellationToken);
                                        Calendar.CurrentDate[chat.Id.ToString()] = DateTime.Today;
                                    }
                                    break;

                                case "calendar":
                                    telegramManager.GetCalendarAsync(botClient, update,chat.Id);
                                    break;

                                case "next":                                    
                                    calendar.HandleCalendarCallback(botClient, callbackQuery);
                                    break;

                                case "prev":                                    
                                    calendar.HandleCalendarCallback(botClient, callbackQuery);
                                    break;
                            }                        
                            await botClient.AnswerCallbackQuery(callbackQuery.Id);
                        }
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при обработке сообщения: {ex.Message}");
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region Конструкторы
        public UpdateHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }
        #endregion
    }
}