﻿using System.Reflection;
using System.Text.Json.Serialization;

namespace RedditArchiver.Models
{
    public class RedditSettings
    {
        public Credentials UserCredentials { get; set; }
        public CrosspostSettings Crosspost { get; set; }
        public string UserAgent { get; set; } = $"windows:reddit-save-archiver:v{Assembly.GetExecutingAssembly().GetName().Version}";

        public class CrosspostSettings
        {
            public bool EnableCrossposting { get; set; } = false;
            public string CrosspostSubreddit { get; set; }
            public bool UseUserAccount { get; set; } = false;
            public Credentials BotCredentials { get; set; }
        }
    }

    public class Credentials
    {
        public string AppID { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class ConnectionStrings
    {
        public string SqliteLocation { get; set; } = "savedPosts.db";
    }
}
