using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Drawing.Imaging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Telegram_Bot_Explosion
{
    class Program
    { 
        static void Main(string[] args)
        {
            StartBot();
        }

        private static string GetToken()
        {
            using (FileStream fstream = System.IO.File.OpenRead("token"))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
                return Encoding.Default.GetString(buffer);
            }
        }

        private static readonly string token = GetToken();
        

        static ITelegramBotClient bot = new TelegramBotClient(token);
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine("Пришло новое сообщение.");
            var message = update.Message;
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (message.Text != null)
                {
                    MessageHandler(message, botClient);
                }
                if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                {
                    FileHandlerExpl(message, botClient);
                }
            }
        }
        
        public static async void MessageHandler(Message message, ITelegramBotClient botClient)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(message.From.FirstName + " написал: " + message.Text));
            if (message.Text.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Привет.");
            }
        }


        public static async void FileHandlerExpl(Message message, ITelegramBotClient botClient)
        {
            Console.WriteLine(message.Photo.Length);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(message.From.FirstName + " отправил фото."));
            string photoName = message.Photo[message.Photo.Length - 1].FileId;
            var file = await bot.GetFileAsync(photoName);
            FileStream fs = new FileStream("ffmpeg/" + photoName + ".jpg", FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
            CutImage(photoName);
            MakeExplMp4(photoName);
            try
            {
                using (var stream = System.IO.File.OpenRead("ffmpeg/" + photoName + ".mp4"))
                {
                    InputOnlineFile iof = new InputOnlineFile(stream);
                    iof.FileName = "videoBoom.mp4";
                    var send = await bot.SendDocumentAsync(message.Chat.Id, iof, "Бум)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public static void StartBot()
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.Read();
        }

        public static void MakeExplMp4(string photoName)
        {
            Execute("ffmpeg/ffmpeg.exe", "-i ffmpeg/"+ photoName + "640.jpg -i ffmpeg/expl3.mp4  -filter_complex [1:v]colorkey=0x329419:0.15:0.15[ckout];[0:0][ckout]overlay=(W-w)/2:(H-h)/3[out] -map [out] -t 5.5 -c:a copy -c:v libx264 -y " +
                "ffmpeg/" + photoName + ".mp4");
            //ffmpeg for videos or gifs
            //ffmpeg -i testtt.mp4 -vf scale=640x640:flags=lanczos -c:v libx264 -preset slow -crf 21 text.mp4
        }

        public static void CutImage(string photoName)
        {
            Image image = Image.FromFile("ffmpeg/" + photoName + ".jpg");
            Bitmap bitmap = new Bitmap(image, new Size(640, 640));
            bitmap.Save("ffmpeg/" + photoName + "640.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private static string Execute(string exePath, string parameters)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
            startInfo.Arguments = parameters;
            startInfo.RedirectStandardOutput = true;

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