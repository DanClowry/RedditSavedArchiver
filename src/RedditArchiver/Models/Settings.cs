namespace RedditArchiver.Models
{
    public class RedditSettings
    {
        public Credentials UserCredentials { get; set; }
        public CrosspostSettings Crosspost { get; set; }
        public string UserAgent { get; set; } = "windows:reddit-save-archiver:v0.1.0";

        public class Credentials
        {
            public string AppID { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }

        public class CrosspostSettings
        {
            public bool EnableCrossposting { get; set; } = false;
            public string CrosspostSubreddit { get; set; }
            public bool UseUserAccount { get; set; } = false;
            public Credentials BotCredentials { get; set; }
        }
    }

    public class ConnectionStrings
    {
        public string SqliteLocation { get; set; } = "savedPosts.db";
    }
}
