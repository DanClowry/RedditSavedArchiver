using Reddit.Controllers;
using System;
using System.Collections.Generic;

namespace RedditArchiver
{
    class Program
    {
        private static Account _account;

        static void Main(string[] args)
        {
            _account = ConfigureAccount();
            List<Post> posts = _account.GetAllSavedPosts();
            foreach (var post in posts)
            {
                Console.WriteLine(post.Fullname + post.Title);
            }
        }

        private static Account ConfigureAccount()
        {
            string appId = Environment.GetEnvironmentVariable("REDDIT_ARCHIVER_APPID");
            string accessToken = Environment.GetEnvironmentVariable("REDDIT_ARCHIVER_ACCESSTOKEN");
            string refreshToken = Environment.GetEnvironmentVariable("REDDIT_ARCHIVER_REFRESHTOKEN");
            string userAgent = Environment.GetEnvironmentVariable("REDDIT_ARCHIVER_USERAGENT");

            return new Account(appId, accessToken, refreshToken, userAgent);
        }
    }
}
