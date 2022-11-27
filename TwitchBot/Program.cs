using System.Net;
using System.Speech.Synthesis;
using AsyncAwaitBestPractices;
using MediaInfo;
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
                // DisplayMessage
                // Console.WriteLine($"{twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");
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
        }


     
        
        

        private static void PlayRandomVideo()
        {
           
            var files = Directory.GetFiles("clips", "*.mp4");
            int index = new Random().Next(0, files.Length);
            var randomfile = files[index];
            var url = randomfile.Replace('\\',Path.AltDirectorySeparatorChar);
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