//
// using System.Windows.Forms.VisualStyles;
// using MediaToolkit;
// using MediaToolkit.Model;
//
// namespace TwtichBot;
//
// public class Programm2
// {
//     public static void Main(String[] args)
//     {
//         
//         // var dur = GetVideoDuration();
//         PlayVideo(@"C:/gitrepos/twitchbot/TwitchBot/TwitchBot/test.mp4");
//
//     }
//     
//     
//     
//     
//     public static void PlayVideo(string url)
//     {
//         var duration = GetMp4DurationInSeconds(url);
//         var template = File.ReadAllText("videotemplate.html");
//         template.Replace("%s", url);
//         File.WriteAllText("video.html",template);
//         Thread.Sleep(1000);
//         var empty = File.ReadAllText("emptytemplate.html");
//         File.WriteAllText("video.html", empty);
//         Thread.Sleep(duration*1000);
//
//
//     }
//
//     public static int GetMp4DurationInSeconds(string url)
//     {
//         var inputFile = new MediaFile { Filename = @"C:/gitrepos/twitchbot/TwitchBot/TwitchBot/test.mp4" };
//
//         using (var engine = new Engine())
//         {
//             engine.GetMetadata(inputFile);
//         }
//         var t = inputFile.Metadata.Duration;
//         Console.WriteLine(inputFile.Metadata.Duration);
//         Console.WriteLine(t.Seconds);
//         return t.Seconds;
//     }
//
//     // var auf = "{";
//         // var zu = "}";
//         // string html = $@"
//         //     <video autoplay src=""{url}"" id=""myVideo"" width=""100%"" height=""100%"">
//         //         video not supported
//         //     </video>
//         //     <script type='text/javascript'>
//         //         document.getElementById('myVideo').addEventListener('ended',myHandler,false);
//         //         function myHandler(e) {auf}
//         //                 window.location.reload();
//         //     {zu}
//         //     </script>";
//         //     
//         // using(StreamWriter sw = File.AppendText("video.html"))
//         // {
//         //     sw.WriteLine(html);
//         //     sw.Close();
//         // }
//
//     }