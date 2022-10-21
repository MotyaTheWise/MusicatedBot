using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using MusicatedBot;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using YoutubeExplode;

namespace TelegramBotExperiments
{

    class Program
    {


        static ITelegramBotClient bot = new TelegramBotClient("tgAPI");
        const long FileSize = 52428800;
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
                    await botClient.SendTextMessageAsync(message.Chat, "Привет! Я MusicatedBot. \n Пришли мне ссылку на видео с ютуба, а я скачаю его для тебя, либо пришлю звуковую дорожку. \n\n Узнать список команд /help");
                    return;
                }

            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message != null && message.Text != null && message.Text.Contains("/t") && ((message.Text.Contains("youtube") ^ message.Text.Contains("m.youtube.com/") ^ message.Text.Contains("youtu.be")))) //send track
                {
                    var chatId = message.Chat.Id;
                    VideoURL = Convert.ToString(message.Text);
                    Console.WriteLine("~~~Link received~~~");
                    try
                    {
                        await DownloadMusic(VideoURL, message, botClient, update, cancellationToken);
                        return;
                    }
                    catch (Exception)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Что-то пошло не так, попробуй еще раз.");
                        return;
                    }

                }

                if (message != null && message.Text != null && message.Text.Contains("/v") && ((message.Text.Contains("youtube") ^ message.Text.Contains("m.youtube.com/") ^ message.Text.Contains("youtu.be")))) //send video
                {
                    var chatId = message.Chat.Id;
                    VideoURL = Convert.ToString(message.Text);
                    Console.WriteLine("~~~Link received~~~");
                    try
                    {
                        await DownloadVideo(VideoURL, message, botClient, update, cancellationToken);
                        return;
                    }
                    catch (Exception)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Что-то пошло не так, попробуй еще раз.");
                        return;
                    }

                }

                if (message != null && message.Text != null && !message.Text.Contains("/t") && !message.Text.Contains("/v") && !message.Text.Contains("help") && !message.Text.Contains("gif"))  //send gif
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Что-то не так. Доступные команды /help");
                    return;
                }

                if (message != null && message.Text != null && message.Text.Contains("/gif"))
                {
                    var chatId = message.Chat.Id;

                    string memeSubject = Convert.ToString(message.Text);
                    memeSubject = memeSubject.Remove(0, 3);
                    Console.WriteLine("~~~Gif requested~~~");
                    try
                    {
                        if (!String.IsNullOrEmpty(memeSubject))
                        {

                            var giphy = new Giphy("K3Cs8PANHoZ5YrO6ltlBmu1P6NKpU12w");
                            var gifresult = await giphy.RandomGif(new RandomParameter()
                            {
                                Tag = memeSubject
                            });
                            string? gifUrl = gifresult?.Data?.Images?.Downsized?.Url;
                            Console.WriteLine(gifresult);
                            if (!String.IsNullOrEmpty(gifUrl))
                            {
                                Message message1 = await botClient.SendAnimationAsync(
                                     chatId: chatId,
                                     animation: gifresult?.Data?.Images?.Downsized?.Url);
                                return;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "ОДОБРЯЕМ!");
                                return;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Что-то не так. Доступные команды /help");
                        return;
                    }

                }

                if (message != null && message.Text != null && message.Text.Contains("/help"))
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Список доступных команд: \n /t *link* – скачать звук из видео (пример: /t https://youtu.be/FzJ_XELywic) \n /v *link* – скачать видео с ютуба (пример: /v https://youtu.be/FzJ_XELywic)\n /gif – получить случайную гифку \n /gif *описание* – получить случайную гифку по указанному запросу (пример: /gif cat) \n\n P.S. если файл из видео получается больше 50MB, то он не будет отправлен.");
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
            var video = await youtube.Videos.GetAsync(VideoURL);         //getting title
            var title = video.Title;

            await botClient.SendTextMessageAsync(message.Chat, $"Видео скачивается...");
            Console.WriteLine("Downloading video...");

            string saveVideoToFolder = "compressed";

            await Saver.SaveVideoAsync(saveVideoToFolder, VideoURL, "video");
            await using Stream stream = System.IO.File.OpenRead("compressed\\video.webm");
            System.IO.FileInfo videoFile = new FileInfo("compressed\\video.webm");
            long size = videoFile.Length;
            Console.WriteLine($"Развер файла в байтах: {size}");
            if (size < FileSize)
            {
                Message message1 = await botClient.SendVideoAsync(
                chatId: chatId,
                video: new InputOnlineFile(content: stream, fileName: $"{title}.webm"));

            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, $"Файл слишком большой, отправка невозможна.");
            }

            return;
        }

        public static async Task DownloadMusic(string VideoURL, Message message, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //download track
        {

            await botClient.SendTextMessageAsync(message.Chat, $"Трек скачивается...");
            Console.WriteLine("Downloading track...");
            var chatId = message.Chat.Id;

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(VideoURL);         //getting title
            var title = video.Title;

            string saveToFolder = "compressed";    //file path on server

            await Saver.SaveTrackAsync(saveToFolder, VideoURL, "track");
            await using Stream stream = System.IO.File.OpenRead("compressed\\track.mp3");
            System.IO.FileInfo audioFile = new FileInfo("compressed\\track.mp3");
            long size = audioFile.Length;
            Console.WriteLine($"Развер файла в байтах: {size}");
            if (size < FileSize)
            {
                Message message1 = await botClient.SendDocumentAsync(
                chatId: chatId,
                document: new InputOnlineFile(content: stream, fileName: $"{title}.mp3"));
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, $"Файл слишком большой, отправка невозможна.");
            }
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