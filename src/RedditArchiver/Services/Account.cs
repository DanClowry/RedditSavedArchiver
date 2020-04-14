using Microsoft.Extensions.Options;
using Reddit;
using Reddit.Controllers;
using RedditArchiver.Models;
using System.Collections.Generic;
using System.Linq;

namespace RedditArchiver.Services
{
    public class Account : IAccount
    {
        private RedditClient _client;

        public Account(IOptions<RedditSettings> options)
        {
            var settings = options.Value;
            _client = new RedditClient(appId: settings.AppID,
                accessToken: settings.AccessToken,
                refreshToken: settings.RefreshToken,
                userAgent: settings.UserAgent);
        }

        public List<Post> GetAllSavedPosts()
        {
            List<Post> savedPosts = new List<Post>();
            savedPosts.AddRange(_client.Account.Me.GetPostHistory("saved", limit: 100));

            List<Post> tempSaved;
            // Continue getting the next group of saved posts until the API returns no more results
            while ((tempSaved =
                _client.Account.Me.GetPostHistory("saved", limit: 100, after: savedPosts.Last().Fullname)).Count != 0)
            {
                savedPosts.AddRange(tempSaved);
            }

            return savedPosts;
        }

        public List<Post> GetSavedPostsBefore(string beforeName)
        {
            List<Post> savedPosts = _client.Account.Me.GetPostHistory("saved", limit: 100, before: beforeName);

            if (savedPosts.Count < 100)
            {
                return savedPosts;
            }

            // Manually search the saved posts list because the Reddit API doesn't properly work if the
            // anchor point is not in the most recent 100 saved posts.
            savedPosts = GetAllSavedPosts();
            int anchorIndex = savedPosts.FindIndex(p => p.Fullname == beforeName);
            if (anchorIndex == -1)
            {
                return new List<Post>();
            }
            return savedPosts.Take(anchorIndex).ToList();
        }
    }
}
