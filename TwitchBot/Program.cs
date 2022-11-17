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
        public static SpeechSynthesizer synthesizer;
        
        static async Task Main(string[] args)
        {
            string text = File.ReadAllText("urmum.txt");  
            string password = text;
            string botUsername = "jenkinsclbot";

            var twitchBot = new TwitchBot(botUsername, password);
            twitchBot.Start().SafeFireAndForget();
            
            await twitchBot.JoinChannel("skyliife");
            await twitchBot.SendMessage("skyliife", "Hey my bot has started up");
            InitVoice();

            twitchBot.OnMessage += async (sender, twitchChatMessage) =>
            {
                Console.WriteLine($"{twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");
                
                if (twitchChatMessage.Message.StartsWith("!hey"))
                {
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"Hey there {twitchChatMessage.Sender}");
                }
                else if (twitchChatMessage.Message.StartsWith("!tts"))
                {
                    var data = twitchChatMessage.Message;
                    PlayMp3FromUrl($"https://api.streamelements.com/kappa/v2/speech?voice=Brian&text={data[4..]}");
                }
                else if (twitchChatMessage.Message.StartsWith("!pic"))
                {
                    var msg = twitchChatMessage.Message;
                    var link = msg.Substring(5);
                    Console.WriteLine(link);
                    PlayVideo(link);
                }
            };
            
            await Task.Delay(-1);
        }


        public static void InitVoice()
        {
            synthesizer = new SpeechSynthesizer();
            synthesizer.Volume = 100;  
            synthesizer.Rate = -2;     
        }
        
        public static void PlayVideo(string url)
        {
            var duration = GetMp4DurationInSeconds(url);
            var template = File.ReadAllText("videotemplate.html");
            var newtemp = template.Replace("%s", url);
            // Console.WriteLine(template);
            // Console.WriteLine(newtemp);
            File.WriteAllText("video.html",newtemp);
            Thread.Sleep(1000);
            var empty = File.ReadAllText("emptytemplate.html");
            File.WriteAllText("video.html", empty);
            Thread.Sleep(duration*1000);
            Console.Write("Finish "+ "slept "+ duration*1000);
            
        }

        public static int GetMp4DurationInSeconds(string url)
        {
            var inputFile = new MediaFile { Filename = url };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
            }
            var t = inputFile.Metadata.Duration;
            Console.WriteLine(inputFile.Metadata.Duration);
            Console.WriteLine(t.Seconds);
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