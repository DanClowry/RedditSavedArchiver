using Microsoft.Extensions.Options;
using Reddit.Controllers;
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

            PostDTO latestSaved = await _database.GetLatestSavedPostAsync();
            List<Post> posts = new List<Post>();
            if (latestSaved != null)
            {
                posts = _userAccount.GetSavedPostsBefore(latestSaved.Fullname);
            }
            else
            {
                posts = _userAccount.GetAllSavedPosts();
            }
            posts.Reverse();
            stopwatch.Stop();
            Console.WriteLine($"Got {posts.Count} newly saved posts from Reddit API. Took {stopwatch.Elapsed.TotalSeconds} seconds.");
            stopwatch.Restart();

            Crosspost(posts);
            Console.WriteLine($"Crossposted {posts.Count} posts to {_settings.Crosspost.CrosspostSubreddit}. Took {stopwatch.Elapsed.TotalSeconds} seconds.");
            stopwatch.Restart();

            await _database.SavePostsAsync(posts, reverse: false);
            stopwatch.Stop();
            Console.WriteLine($"Added {posts.Count} posts to the database. Took {stopwatch.Elapsed.TotalSeconds} seconds.");
        }

        private void Crosspost(List<Post> posts)
        {
            if (_botAccount != null)
            {
                string crosspostSub = _settings.Crosspost.CrosspostSubreddit;
                if (!_botAccount.ModeratesSubreddit(crosspostSub))
                {
                    Console.WriteLine("Bot account must be a moderator of the crosspost subreddit.");
                    return;
                }

                foreach (var post in posts)
                {
                    _botAccount.SubmitCrosspost(post, crosspostSub);
                }
            }
        }
    }
}
