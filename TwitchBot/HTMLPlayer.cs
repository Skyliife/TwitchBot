using System.Security.Cryptography.X509Certificates;

namespace TwtichBot;

public class HTMLPlayer
{
    public string hidden = "hidden";
    public HTMLPlayer()
    {
        var auf = "{";
        var zu = "}";
        hidden = "";
        string html = $@"<!doctype html>

        
<html>
<head>
    <title>Our Funky HTML Page</title>
    <meta name=""description"" content=""Our first page"">
    <meta name=""keywords"" content=""html tutorial template"">
    <meta http-equiv=""refresh"" content=""2"">
</head>
<body>
<!-- <img src=""https://i.imgur.com/r7H9Ilu.mp4"" alt=""this slowpoke moves""  width=""250"" /> -->
            <video {hidden} id=""video"" width=""800"" height=""600"" autoplay muted onended=""hideElem()"">
            <source src=""https://i.imgur.com/r7H9Ilu.mp4"" type=""video/mp4"">
    
                                                               Your browser does not support the video tag.
</video>
</body>
<script>
    function hideElem() {auf}
        document.getElementById(""video"").style.visibility = ""hidden"";
        {zu}
</script>
</html>

";

        File.WriteAllText(@"C:/gitrepos/twitchbot/TwitchBot/TwitchBot/bin/Debug/net6.0/test.html", html);
        string text = File.ReadAllText("test.html");
        Console.WriteLine(text);
    }
}