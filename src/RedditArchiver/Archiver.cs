using Microsoft.Extensions.Options;
using Reddit.Controllers;
using Reddit.Exceptions;
using RedditArchiver.Data;
using RedditArchiver.Models;
using RedditArchiver.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RedditArchiver
{
    public class Archiver
    {
        private readonly IDataStore _database;
        private readonly IUserAccount _userAccount;
        private readonly IBotAccount _botAccount;
        private readonly RedditSettings _settings;

        public Archiver(IDataStore dataStore, IUserAccount userAccount, IOptions<RedditSettings> settings)
            : this(dataStore, userAccount, null, settings) { }

        public Archiver(IDataStore dataStore, IUserAccount userAccount, IBotAccount botAccount, IOptions<RedditSettings> settings)
        {
            _database = dataStore;
            _userAccount = userAccount;
            _botAccount = botAccount;
            _settings = settings.Value;
        }

        public async Task Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Post> posts = await GetPostsFromReddit();
            stopwatch.Stop();
            Console.WriteLine($"Got {posts.Count} newly saved posts from Reddit API. Took {stopwatch.Elapsed.TotalSeconds} seconds.");
            stopwatch.Restart();

            await ArchivePosts(posts);
        }

        private async Task<List<Post>> GetPostsFromReddit()
        {
            PostDTO latestSaved = await _database.GetLatestSavedPostAsync();
            List<Post> posts = new List<Post>();
            try
            {
                if (latestSaved != null)
                {
                    posts = _userAccount.GetSavedPostsBefore(latestSaved.Fullname);
                }
                else
                {
                    posts = _userAccount.GetAllSavedPosts();
                }
            }
            catch (RedditUnauthorizedException)
            {
                Console.WriteLine("Got unauthorised response from Reddit when attempting to retrieve saved posts. " +
                    "Check that the app has been authorised to access your account (reddit.com/prefs/apps/) and " +
                    "that your user credentials in appsettings.json are correct.");
                Environment.Exit(1);
            }
            posts.Reverse();
            return posts;
        }

        private async Task ArchivePosts(List<Post> posts)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (_botAccount != null)
            {
                string crosspostSub = _settings.Crosspost.CrosspostSubreddit;
                if (String.IsNullOrWhiteSpace(crosspostSub))
                {
                    Console.WriteLine("Crosspost subreddit must not be empty when crossposting is enabled.\n" +
                        "Please set the CrosspostSubreddit value in your appsettings.json.");
                    Environment.Exit(1);
                }

                try
                {
                    if (_botAccount.ModeratesSubreddit(crosspostSub))
                    {
                        foreach (var post in posts)
                        {
                            List<Task> tasks = new List<Task>();
                            tasks.Add(_botAccount.SubmitCrosspostAsync(post, crosspostSub));
                            tasks.Add(_database.SavePostAsync(post));
                            await Task.WhenAll(tasks);
                            Console.WriteLine($"\"{post.Title}\" saved to database and crossposted to /r/{_settings.Crosspost.CrosspostSubreddit}");
                        }
                        stopwatch.Stop();
                        Console.WriteLine($"\nSaved and crossposted {posts.Count} posts in {stopwatch.Elapsed.TotalSeconds:N0} seconds");
                    }
                    else
                    {
                        Console.WriteLine("Bot account must be a moderator of the crosspost subreddit.");
                    }
                }
                catch (RedditUnauthorizedException)
                {
                    Console.WriteLine("Got unauthorised response from Reddit when attempting to crosspost. " +
                    "Check that the app has been authorised to access the bot account (reddit.com/prefs/apps/) and " +
                    "that the bot credentials in appsettings.json are correct.");
                    Environment.Exit(1);
                }
            }
            else
            {
                await _database.SavePostsAsync(posts, reverse: false);
                stopwatch.Stop();
                Console.WriteLine($"\nSaved {posts.Count} posts to the database in {stopwatch.Elapsed.TotalSeconds:N0} seconds");
            }
        }
    }
}
