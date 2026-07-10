
using Telegram.Bot;

namespace Organizer
{
    public class TelegramBot
    {
        #region Поля и свойства
        /// <summary>
        /// Создание клиента для работы с Телеграм ботом.
        /// </summary>
        private readonly ITelegramBotClient _botClient;
        #endregion

        #region Методы
        /// <summary>
        /// Запускает бота и начинает получать обновления.
        /// </summary>
        /// <param name="cancellationToken">Токен для отмены операции.</param>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var me = await _botClient.GetMe();

            Console.WriteLine($"Бот @{me.Username} запущен. Ожидание сообщений...");

            await _botClient.ReceiveAsync(
              updateHandler: new UpdateHandler(_botClient),
              cancellationToken: cancellationToken
            );
        }
        #endregion

        #region Конструктор
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="botToken">Токен телеграм бота.</param>
        public TelegramBot(string botToken)
        {
            _botClient = new TelegramBotClient(botToken);
        }
        #endregion
    }
}
