using Reddit;
using Reddit.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace RedditArchiver
{
    public class Account
    {
        private RedditClient _client;

        public Account(string appId, string accessToken, string refreshToken, string userAgent)
        {
            _client = new RedditClient(appId: appId, accessToken: accessToken,
                refreshToken: refreshToken, userAgent: userAgent);
        }

        /// <summary>
        /// Gets all saved posts for the current user's account.
        /// </summary>
        /// <remarks>Does not include saved comments</remarks>
        /// <returns>A list of posts the user has saved.</returns>
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

        /// <summary>
        /// Gets all the posts the user has saved after another post.
        /// </summary>
        /// <param name="beforeName">The fullname of the post to use as an anchor point (exclusive). Will only return posts saved later than the anchor post.</param>
        /// <remarks>Fetches all the user's saved posts if the anchor point is not one of the most recent 100 saved posts.
        /// The post used as an anchor point is not included in the returned list of posts.</remarks>
        /// <returns>A list of posts the user has saved. Returns an empty list if the anchor point does not exist in the user's saved posts.</returns>
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
