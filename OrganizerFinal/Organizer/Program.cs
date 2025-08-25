
using Aspose.Pdf;
using BusinessNotes;

namespace Organizer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {            
            try
            {
                Token token = new Token();
                string readerFile = token.GetToken();
                var botService = new TelegramBot(readerFile);
                await botService.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
                Console.WriteLine("Вы ввели не правильный токен, приложение будет перезапущено, " +
                  "введите токен правильно");
            }
            
        }

    }
}
