using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using MediaToolkit.Model;
using MusicatedBot;
using YoutubeExplode;
using YoutubeExplode.Converter;
using Microsoft.VisualBasic;
using Telegram.Bot.Types.InputFiles;
using FFMpegSharp;
using FFMpegSharp.FFMPEG;

namespace TelegramBotExperiments
{

    class Program
    {


        static ITelegramBotClient bot = new TelegramBotClient("insert_API");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string VideoURL = "";
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;

                if (message != null && message.Text != null && message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Привет! Я MusicatedBot. Пришли мне ссылку на видео с ютуба, а я скачаю его для тебя, либо пришлю звуковую дорожку. Узнать список команд /help");
                    return;
                }

            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message != null && message.Text != null && message.Text.Contains("/t") && ((message.Text.Contains("youtube") ^ message.Text.Contains("https://m.youtube.com/") ^ message.Text.Contains("youtu.be"))))
                {
                    var chatId = message.Chat.Id;
                    VideoURL = Convert.ToString(message.Text);
                    Console.WriteLine("~~~Link received~~~");
                    await DownloadMusic(VideoURL, message, botClient, update, cancellationToken);
                    return;
                }

                if (message != null && message.Text != null && message.Text.Contains("/v") && ((message.Text.Contains("youtube") ^ message.Text.Contains("https://m.youtube.com/") ^ message.Text.Contains("youtu.be"))))
                {
                    var chatId = message.Chat.Id;
                    VideoURL = Convert.ToString(message.Text);
                    Console.WriteLine("~~~Link received~~~");
                    await DownloadVideo(VideoURL, message, botClient, update, cancellationToken);
                    return;
                }

                if (message != null && message.Text != null && !message.Text.Contains("/t") && !message.Text.Contains("/v") && !message.Text.Contains("help"))
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Ты прислал неправильную ссылку, либо не указал '/t' или '/v' перед ссылкой.");
                    return;
                }

                if (message != null && message.Text != null && message.Text.Contains("/help")){
                    await botClient.SendTextMessageAsync(message.Chat, "Список доступных команд: \n /t *link* – скачать звук из видео \n /v *link* – скачать видео с ютуба (длительностью не более 2 минут) \n /meme – получить рофлинку (пока не работает)");
                    return;
                }

            }
        }



        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public static async Task DownloadVideo(string VideoURL, Message message, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //download video
        {

            var chatId = message.Chat.Id;

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(VideoURL);         //getting title, author and duration
            var title = video.Title;
            var author = video.Author;
            var duration = video.Duration;
            await botClient.SendTextMessageAsync(message.Chat, $"Видео скачивается...");
            Console.WriteLine("Downloading video...");

            string saveVideoToFolder = "compressed";


            await Save.SaveVideoAsync(saveVideoToFolder, VideoURL, "zalupa");
            await using Stream stream = System.IO.File.OpenRead("compressed\\zalupa.webm");
            System.IO.FileInfo file = new FileInfo("compressed\\zalupa.webm");
            long size = file.Length;
            Console.WriteLine(size);
            if (size < 52428800)
            {
                Message message1 = await botClient.SendVideoAsync(
                chatId: chatId,
                video: new InputOnlineFile(content: stream, fileName: $"{title}.webm"));
                
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, $"Файл слишком большой, соси жопу.");
            }

            return;
        }

        public static async Task DownloadMusic(string VideoURL, Message message, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //download track
        {

            await botClient.SendTextMessageAsync(message.Chat, $"Трек скачивается...");
            Console.WriteLine("Downloading track...");
            var chatId = message.Chat.Id;

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(VideoURL);         //getting title, author and duration
            var title = video.Title;
            var author = video.Author;
            var duration = video.Duration;

            string saveToFolder = "compressed";    //file path on server

            await Save.SaveTrackAsync(saveToFolder, VideoURL, "zalupa");
            
            await using Stream stream = System.IO.File.OpenRead("compressed\\zalupa.mp3");
            Message message1 = await botClient.SendDocumentAsync(
                chatId: chatId,
                document: new InputOnlineFile(content: stream, fileName: $"{title}.mp3"));
            return;

        }



        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            if (!Directory.Exists("compressed"))
            {
                Directory.CreateDirectory("compressed");
            }
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
    }
}