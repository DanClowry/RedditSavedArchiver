using Reddit.Controllers;
using System.Collections.Generic;

namespace RedditArchiver.Services
{
    public interface IUserAccount
    {
        /// <summary>
        /// Gets all saved posts for the current user's account.
        /// </summary>
        /// <remarks>Does not include saved comments</remarks>
        /// <returns>A list of posts the user has saved.</returns>
        public List<Post> GetAllSavedPosts();

        /// <summary>
        /// Gets all the posts the user has saved after another post.
        /// </summary>
        /// <param name="beforeName">The fullname of the post to use as an anchor point (exclusive). Will only return posts saved later than the anchor post.</param>
        /// <remarks>Fetches all the user's saved posts if the anchor point is not one of the most recent 100 saved posts.
        /// The post used as an anchor point is not included in the returned list of posts.</remarks>
        /// <returns>A list of posts the user has saved. Returns an empty list if the anchor point does not exist in the user's saved posts.</returns>
        public List<Post> GetSavedPostsBefore(string beforeName);
    }
}
