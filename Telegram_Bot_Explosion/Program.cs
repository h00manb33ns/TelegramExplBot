using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Drawing.Imaging;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Telegram_Bot_Explosion
{
    class Program
    {
        
        static ITelegramBotClient bot = new TelegramBotClient("5634788754:AAGr1z0SWHyoFYgh5RP-DNHBh6MHPR6fqBY");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //makeExplMp4();
            sendExpl(update, botClient);

            
        }
        
        public static async Task MessageHandler(Update update, ITelegramBotClient botClient)
        {

        }


        public static async Task sendExpl(Update update, ITelegramBotClient botClient)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text != null)
                    if (message.Text.ToLower() == "/start")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Привет.");
                        return;
                    }
                if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                {
                    string photoName = message.Photo[message.Photo.Length - 1].FileId;
                    var file = await bot.GetFileAsync(photoName);
                    FileStream fs = new FileStream("ffmpeg/" + photoName + ".jpg", FileMode.Create);
                    await bot.DownloadFileAsync(file.FilePath, fs);
                    fs.Close();
                    fs.Dispose();
                    cutImage(photoName);
                    makeExplMp4(photoName);
                    //await botClient.SendVideoAsync(message.Chat, "ffmpeg/result7.mp4");
                    try
                    {
                        using (var stream = System.IO.File.OpenRead("ffmpeg/"+ photoName + ".mp4"))
                        {
                            InputOnlineFile iof = new InputOnlineFile(stream); //оставляем также 
                            iof.FileName = "videoBoom.mp4";
                            var send = await bot.SendDocumentAsync(update.Message.Chat.Id, iof, "Бум)");
                        }
                    }
                    catch
                    {

                    }
                    //await botClient.SendVideoAsync(update.Message.Chat.Id, video: "ffmpeg/result7.mp4", caption: "Бум)");
                }
                //try
                //{
                //    await botClient.SendTextMessageAsync(message.Chat, "М)");
                //}
                //catch { }
            }
            //makeExplMp4();
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }



        static void Main(string[] args)
        {
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
            Console.ReadKey();
            //cutImage();
            //makeExplMp4();
        }
        public static async Task makeExplMp4(string photoName)
        {
            Execute("ffmpeg/ffmpeg.exe", "-i ffmpeg/"+ photoName + "640.jpg -i ffmpeg/expl3.mp4  -filter_complex [1:v]colorkey=0x329419:0.15:0.15[ckout];[0:0][ckout]overlay=(W-w)/2:(H-h)/3[out] -map [out] -t 5.5 -c:a copy -c:v libx264 -y " +
                "ffmpeg/" + photoName + ".mp4");
            //ffmpeg -loop 1 -i image.png -i video.mp4 -filter_complex [1:v]colorkey=0x000000:0.1:0.1[ckout];[0:v][ckout]overlay[out] -map [out] -t 5 -c:a copy -c:v libx264 -y result.mp4

            //ffmpeg for videos or gifs
            //ffmpeg -i testtt.mp4 -vf scale=640x640:flags=lanczos -c:v libx264 -preset slow -crf 21 text.mp4
        }

        public static void cutImage(string photoName)
        {
            Image image = Image.FromFile("ffmpeg/" + photoName + ".jpg");
            Bitmap bitmap = new Bitmap(image, new Size(640, 640)); // or some math to resize it to 1/2
            bitmap.Save("ffmpeg/" + photoName + "640.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private static string Execute(string exePath, string parameters)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
            startInfo.Arguments = parameters;
            startInfo.RedirectStandardOutput = true;
            //startInfo.RedirectStandardError = true;

            Console.WriteLine(string.Format(
                "Executing \"{0}\" with arguments \"{1}\".\r\n",
                startInfo.FileName,
                startInfo.Arguments));

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = process.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //Console.ReadKey();
            return "0";
        }
    }
}