using Reddit.Controllers;
using RedditArchiver.Data;
using RedditArchiver.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RedditArchiver
{
    class Program
    {
        private static Account _account;
        private static IDataStore _database;

        static async Task Main(string[] args)
        {
            _account = ConfigureAccount();
            _database = new SqlLiteDataStore();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            PostDTO latestSaved = await _database.GetLatestSavedPostAsync();
            List<Post> posts = new List<Post>();
            if (latestSaved != null)
            {
                posts = _account.GetSavedPostsBefore(latestSaved.Fullname);
            }
            else
            {
                posts = _account.GetAllSavedPosts();
            }
            await _database.SavePostsAsync(posts);

            stopwatch.Stop();
            Console.WriteLine($"Added {posts.Count} new posts to the database. Took {stopwatch.Elapsed.TotalSeconds} seconds.");
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
