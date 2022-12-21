using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Windows.Forms;
using AsyncAwaitBestPractices;
using MediaToolkit;
using MediaToolkit.Model;
using NAudio.Wave;

namespace TwtichBot
{
    class Program
    {
        public static Config Config { get; private set; } = null;
        static async Task Main(string[] args)
        {
            Config = Config.LoadOrDefault();
            string botUsername = Config.Username;
            string password = Config.AccessToken;
            string channel = Config.Channel;
            // Console.Write(password);
            var twitchBot = new TwitchBot(botUsername, password);
            twitchBot.Start().SafeFireAndForget();

            await twitchBot.JoinChannel(channel);
            await twitchBot.SendMessage(channel, "Hey my bot has started up");


            twitchBot.OnMessage += async (sender, twitchChatMessage) =>
            {
                await HandleMessages(twitchChatMessage, twitchBot);
            };

            await Task.Delay(-1);
        }

        private static async Task HandleMessages(TwitchBot.TwitchChatMessage twitchChatMessage, TwitchBot twitchBot)
        {
            if (twitchChatMessage.Message.StartsWith("!hey"))
            {
                await twitchBot.SendMessage(twitchChatMessage.Channel, $"Hey there {twitchChatMessage.Sender}");
            }
            else if (twitchChatMessage.Message.StartsWith("!tts"))
            {
                var data = twitchChatMessage.Message;
                PlayMp3FromUrl($"https://api.streamelements.com/kappa/v2/speech?voice=Brian&text={data[4..]}");
            }
            else if (twitchChatMessage.Message.StartsWith("!clip"))
            {
                PlayRandomVideo();
            }
            else if (twitchChatMessage.Message.StartsWith("!5head"))
            {
                RunFiveHead();
            }
            else if (twitchChatMessage.Message.StartsWith("!clown"))
            {
                RunClown();
            }
            else if (twitchChatMessage.Message.StartsWith("!gif"))
            {
                Console.WriteLine(Config.Gifsupport);
                if (Config.Gifsupport)
                {
                    var data = twitchChatMessage.Message;
                    if (data.Split(' ').Length == 2 && data.EndsWith(".gif"))
                    {
                        PlayGif(data);
                    }
                    else
                    {
                        await twitchBot.SendMessage(twitchChatMessage.Channel, "File is not supported please provide a .gif fileformat");
                    }
                }
                else
                {
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"{twitchChatMessage.Sender} gif support is disabled by the broadcaster.");
                }
            }
        }

        private static void RunClown()
        {
            Process p = Process.GetProcessesByName("Snap Camera").FirstOrDefault();
            
            if (p != null)
            {
                SendKeys.SendWait("^{3}");
                Thread.Sleep(5000);
                SendKeys.SendWait("^{3}");
            }
            
        }

        private static void RunFiveHead()
        {
            SendKeys.Send("{^2}");
            Thread.Sleep(5000);
            SendKeys.Send("{^2}");
        }

        private static void PlayGif(string msg)
        {
            Console.WriteLine(msg.Split(' ').Length);

            var url = msg.Split(' ')[1];
            var stream = GetStreamFromUrl(url);
            Image img = Image.FromStream(stream);
            var duration = GetGifDuration(img);
            Console.Write("DURATION: " + duration.Value.Seconds);
            var template = File.ReadAllText("html/giftemplate.html");
            var newtemp = template.Replace("%gif", @$"{url}").Replace("%secs", @$"{5000}");
            // Console.WriteLine(template);
            Console.WriteLine(newtemp);
            File.WriteAllText("html/video.html", newtemp);
            Thread.Sleep(1000);
            var empty = File.ReadAllText("html/emptytemplate.html");
            Thread.Sleep(duration.Value.Seconds * 1000);
            File.WriteAllText("html/video.html", empty);
            Thread.Sleep(duration.Value.Seconds * 1000);
        }
    

    private static Stream GetStreamFromUrl(string url)
    {
        byte[] imageData = null;

        using (var wc = new WebClient())
            imageData = wc.DownloadData(url);

        return new MemoryStream(imageData);
    }


    public static TimeSpan? GetGifDuration(Image image, int fps = 60)
    {
        var minimumFrameDelay = (1000.0 / fps);
        if (!image.RawFormat.Equals(ImageFormat.Gif)) return null;
        if (!ImageAnimator.CanAnimate(image)) return null;

        var frameDimension = new FrameDimension(image.FrameDimensionsList[0]);

        var frameCount = image.GetFrameCount(frameDimension);
        var totalDuration = 0;

        for (var f = 0; f < frameCount; f++)
        {
            var delayPropertyBytes = image.GetPropertyItem(20736).Value;
            var frameDelay = BitConverter.ToInt32(delayPropertyBytes, f * 4) * 10;
            // Minimum delay is 16 ms. It's 1/60 sec i.e. 60 fps
            totalDuration += (frameDelay < minimumFrameDelay ? (int)minimumFrameDelay : frameDelay);
        }

        return TimeSpan.FromMilliseconds(totalDuration);
    }


    private static void PlayRandomVideo()
    {
        var files = Directory.GetFiles("clips", "*.mp4");
        int index = new Random().Next(0, files.Length);
        var randomfile = files[index];
        var url = randomfile.Replace('\\', Path.AltDirectorySeparatorChar);
        // Console.WriteLine(url);
        var duration = GetMp4DurationInSeconds(url);
        var template = File.ReadAllText("html/videotemplate.html");
        var newtemp = template.Replace("%s", @$"../{url}");
        // Console.WriteLine(template);
        // Console.WriteLine(newtemp);
        File.WriteAllText("html/video.html", newtemp);
        Thread.Sleep(1000);
        var empty = File.ReadAllText("html/emptytemplate.html");
        Thread.Sleep(duration * 1000);
        File.WriteAllText("html/video.html", empty);
        Thread.Sleep(duration * 1000);
        // Console.Write("Finish " + "slept " + duration * 1000);
    }

    public static int GetMp4DurationInSeconds(string url)
    {
        var inputFile = new MediaFile { Filename = url };

        using (var engine = new Engine())
        {
            engine.GetMetadata(inputFile);
        }

        var t = inputFile.Metadata.Duration;
        // Console.WriteLine(inputFile.Metadata.Duration);
        // Console.WriteLine(t.Seconds);
        return t.Seconds;
    }

    public static void PlayMp3FromUrl(string url)
    {
        using (Stream ms = new MemoryStream())
        {
            using (Stream stream = WebRequest.Create(url)
                       .GetResponse().GetResponseStream())
            {
                byte[] buffer = new byte[32768];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
            }

            ms.Position = 0;
            using (WaveStream blockAlignedStream =
                   new BlockAlignReductionStream(
                       WaveFormatConversionStream.CreatePcmStream(
                           new Mp3FileReader(ms))))
            {
                using (WaveOutEvent waveOut = new WaveOutEvent())
                {
                    waveOut.Init(blockAlignedStream);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }
    }
}

}