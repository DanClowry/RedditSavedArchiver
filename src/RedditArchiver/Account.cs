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
    }
}
