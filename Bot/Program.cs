using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;

namespace TelegramBotExperiments
{

    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        public static ICollection<string> uris;

        static ITelegramBotClient bot = new TelegramBotClient("6252173871:AAFUgwzkUzuXQu6ZXnLzbZLcmJQf_APEHBI");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/check")
                {
                    for (int i = message.MessageId; i >= 0; i--)
                    {
                        try
                        {
                            await botClient.DeleteMessageAsync(message.Chat.Id, i, cancellationToken);
                        }
                        catch
                        {
                            break;
                        }
                    }
                    try
                    {
                        //var uris = DownloadFiles("http://simfpolyteh.ru/raspisanie/");
                        foreach(string uri in uris)
                        {
                            await botClient.SendPhotoAsync(message.Chat.Id,
                        new Telegram.Bot.Types.InputFiles.InputOnlineFile(uri));
                        }
                        
                        //await botClient.SendPhotoAsync(message.Chat.Id,
                        //    new Telegram.Bot.Types.InputFiles.InputOnlineFile("http://simfpolyteh.ru/wp-content/uploads/2022/11/ZvonkiNEW.jpg"));
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка");
                    }


                }
                //await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!!");
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        static void Main(string[] args)
        {
            uris = DownloadFiles("http://simfpolyteh.ru/raspisanie/");

            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }

        private static ICollection<string> DownloadFiles(string site)
        {
            WebClient client = new WebClient();

            // Получаем содержимое страницы
            string data;
            using (Stream stream = client.OpenRead(site))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
            }

            // Парсим теги изображений
            Regex regex = new Regex(@"\<img.+?src=\""(?<imgsrc>.+?)\"".+?\>", RegexOptions.ExplicitCapture);
            MatchCollection matches = regex.Matches(data);

            // Регекс для проверки на корректную ссылку картинки
            Regex fileRegex = new Regex(@"[^\s\/]\.(jpg|png|gif|bmp)\z", RegexOptions.Compiled);

            // Получаем ссылки на картинки
            var imagesUrl = matches
                .Cast<Match>()
                // Данный из группы регулярного выражения
                .Select(m => m.Groups["imgsrc"].Value.Trim())
                // Добавляем название сайта, если ссылки относительные
                .Select(url => url.Contains("http://") ? url : (site + url))
                // Получаем название картинки
                .Select(url => new { url, name = url.Split(new[] { '/' }).Last() })
                // Проверяем его
                .Where(a => fileRegex.IsMatch(a.name))
                // Удаляем повторяющиеся элементы
                .Distinct()
                ;

            var uris = imagesUrl.Select(i => i.url).ToList();
            return new List<string>() { uris[2], uris[7] };
            
        }
    }
}