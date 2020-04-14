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
        private readonly IAccount _account;

        public Archiver(IDataStore dataStore, IAccount account)
        {
            _database = dataStore;
            _account = account;
        }

        public async Task Run()
        {
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
    }
}
