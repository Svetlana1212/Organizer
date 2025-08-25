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
        /// Проверяет была ли запущена команда /create
        /// </summary>
        public static bool create;

        
        public static string CurrentMessage { get; set; }
        public static string CurrentStatus { get; set; }

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
                                      "Благодаря мне ты можешь:\n" +
                                      " * Создавать заметки\n" +
                                      " * Установливать дату для их просмотра\n" +
                                      " * Редактировать заметки\n", cancellationToken);
                                    telegramManager.MenuOutput(botClient,update, cancellationToken);
                                }
                                else if (text == BotChatCommands.Menu)
                                {
                                    telegramManager.MenuOutput(botClient, update, cancellationToken);
                                }
                                else if (CurrentStatus == "text")
                                {
                                    CurrentMessage = message.Text;
                                }
                                else if ((CurrentStatus == "date") && (CurrentMessage != message.Text))
                                {
                                    await Calendar.SendCalendarAsync(botClient, message.Chat.Id, DateTime.Now);
                                    telegramManager.CreateNotesAsync(botClient, update, chatId, cancellationToken);
                                    CurrentMessage = message.Text;
                                }
                                else if ((CurrentStatus == "newNote") && (message.Text == BotChatCommands.Ok))
                                {
                                    telegramManager.CreateNotesAsync(botClient, update, chatId, cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendMessage(
                                      chatId: chatId,
                                      text: $"Для продолжения работы нажми:/menu\n" +
                                      $"{BotChatCommands.Menu} - это вывод всех доступных команд 🦅\n"

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
                                    telegramManager.NoteAllAsync(botClient,update, cancellationToken);
                                    break;

                                case "listAll":                                    
                                    bool all = true;
                                    telegramManager.NoteAllAsync(botClient, update,cancellationToken, all);
                                    break;

                                case "create":                                                                     
                                    CurrentStatus = "text";                                    
                                    telegramManager.CreateNotesAsync(botClient, update,chat.Id, cancellationToken);
                                    break;

                                case "deleteAll":                                                                    
                                    telegramManager.DeleteNotesAsync(botClient, update, chat.Id, cancellationToken);
                                    break;

                                case "day":                                                                      
                                    calendar.HandleCalendarCallback(botClient, callbackQuery);
                                    CurrentStatus =(CurrentStatus == "date")? "newNote":"";
                                    await botClient.SendMessage(
                                    chatId: chat.Id,
                                    text: $"Вы выбрали дату {Calendar.CurrentDate.ToString("dd MM yyyy")}\n Введите команду /ok"
                                    );
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