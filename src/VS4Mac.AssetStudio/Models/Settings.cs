namespace VS4Mac.AssetStudio.Models
{
    public class Settings
    {
        public string ApiKey { get; set; }

        public static Settings Default()
        {
            return new Settings
            {
                ApiKey = string.Empty
            };
        }
    }
}