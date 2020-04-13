namespace RedditArchiver.Models
{
    public class RedditSettings
    {
        public string AppID { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserAgent { get; set; }
    }

    public class ConnectionStrings
    {
        public string SqliteLocation { get; set; }
    }
}
