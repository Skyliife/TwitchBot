using Newtonsoft.Json;

namespace TwtichBot
{
    class Config
    {
        public string Username { get; set; }
        public string AccessToken { get; set; }
        
        public bool Gifsupport { get; set; }
        public string Channel { get; set; }

        private const string filename = "config.json";

        public static Config LoadOrDefault()
        {
            if (File.Exists(filename))
            {
                return Load();
            }

            Console.WriteLine("Creating new default config file");
            var result = new Config
            {
                Username = "username",
                Channel = "channel",
                AccessToken = "token",
                Gifsupport = false,
            };
            result.Save();
            return result;
        }

        public static Config Load()
        {
            string lines;
            try
            {
                lines = File.ReadAllText(filename);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to load config file", e);
            }

            var result = JsonConvert.DeserializeObject<Config>(lines);
            if (result == null)
            {
                throw new Exception("Unable to deserialize configuration file");
            }

            return result;
        }

        public void Save()
        {
            string jsonstring = JsonConvert.SerializeObject(this, Formatting.Indented);
            try
            {
                File.WriteAllText(filename, jsonstring);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to write configuration file", e);
            }
        }

        public override string ToString()
        {
            return $"User name: {Username}, Channel: {Channel}";
        }
    }
}