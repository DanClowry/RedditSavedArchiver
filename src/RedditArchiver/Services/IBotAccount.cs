using Reddit.Controllers;

namespace RedditArchiver.Services
{
    public interface IBotAccount
    {
        /// <summary>
        /// Submits a crosspost of a post to a subreddit
        /// </summary>
        /// <param name="originalPost">The original post to crosspost</param>
        /// <param name="subreddit">The subreddit to crosspost to</param>
        /// <param name="includeComment">Whether a comment should be added to the post with extra information on the original post</param>
        /// <remarks>Posts are titled as [/r/{originalSubreddit}] {originalTitle}</remarks>
        /// <returns>A post controller for the new post</returns>
        public Post SubmitCrosspost(Post originalPost, string subreddit, bool includeComment = true);

        /// <summary>
        /// Submit a link post in a subreddit
        /// </summary>
        /// <param name="title">The title of the post. Trimmed if greater than 300 characters</param>
        /// <param name="link">The link to post</param>
        /// <param name="subreddit">The subreddit to post to</param>
        /// <param name="nsfw">Whether the post is NSFW</param>
        /// <param name="flair">The flair of the post</param>
        /// <returns>A post controller for the new post</returns>
        public Post SubmitLinkPost(string title, string link, string subreddit, bool nsfw = false, string flair = null);

        /// <summary>
        /// Submit a top-level comment on a post
        /// </summary>
        /// <param name="post">The post to comment on</param>
        /// <param name="commentBody">The body of the comment. Styled using Markdown</param>
        /// <returns>A comment controller for the new comment</returns>
        public Comment SubmitComment(Post post, string commentBody);

        /// <summary>
        /// Check whether the account moderates a subreddit
        /// </summary>
        /// <param name="subreddit">The subreddit name (without /r/) to check</param>
        /// <returns>True if the account is a moderator, false otherwise</returns>
        public bool ModeratesSubreddit(string subreddit);
    }
}
