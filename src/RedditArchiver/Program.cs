using Microsoft.Extensions.Configuration;
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
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables("REDDIT_ARCHIVER_")
                .AddCommandLine(args)
                .Build();
            var redditSettings = new RedditSettings();
            config.Bind("Reddit", redditSettings);
            var connStrings = new ConnectionStrings();
            config.Bind("ConnectionStrings", connStrings);

            _account = ConfigureAccount(redditSettings);
            _database = new SqlLiteDataStore(connStrings);

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

        private static Account ConfigureAccount(RedditSettings settings)
        {
            return new Account(settings.AppID, settings.AccessToken,
                settings.RefreshToken, settings.RefreshToken);
        }
    }
}
