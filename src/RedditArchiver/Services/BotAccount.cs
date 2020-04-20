using Microsoft.Extensions.Options;
using Reddit.Controllers;
using RedditArchiver.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static RedditArchiver.Models.RedditSettings;

namespace RedditArchiver.Services
{
    public class BotAccount : BaseAccount, IBotAccount
    {
        public BotAccount(IOptions<RedditSettings> options, Credentials credentials)
            : base(options.Value, credentials) { }

        public Post SubmitCrosspost(Post originalPost, string subreddit, bool includeComment = true)
        {
            string title = $"[/r/{originalPost.Subreddit}] {originalPost.Title}";
            string fullPermalink = "https://reddit.com" + originalPost.Permalink;

            Post xPost = SubmitLinkPost(title, fullPermalink, subreddit, originalPost.NSFW, originalPost.Subreddit);

            if (includeComment)
            {
                string commentBody = $"# \"{originalPost.Title}\" by /u/{originalPost.Author}\n" +
                    $"## Saved from /r/{originalPost.Subreddit}\n" +
                    $"Original post's linked URL: {originalPost.Listing.URL}\n\n" +
                    $"Original post's fullname: {originalPost.Fullname}";
                SubmitComment(xPost, commentBody);
            }

            return xPost;
        }

        public async Task<Post> SubmitCrosspostAsync(Post originalPost, string subreddit, bool includeComment = true)
        {
            string title = $"[/r/{originalPost.Subreddit}] {originalPost.Title}";
            string fullPermalink = "https://reddit.com" + originalPost.Permalink;

            Post xPost = await SubmitLinkPostAsync(title, fullPermalink, subreddit, originalPost.NSFW, originalPost.Subreddit);

            if (includeComment)
            {
                string commentBody = $"# \"{originalPost.Title}\" by /u/{originalPost.Author}\n" +
                    $"## Saved from /r/{originalPost.Subreddit}\n" +
                    $"Original post's linked URL: {originalPost.Listing.URL}\n\n" +
                    $"Original post's fullname: {originalPost.Fullname}";
                await SubmitCommentAsync(xPost, commentBody);
            }

            return xPost;
        }

        public Post SubmitLinkPost(string title, string link, string subreddit, bool nsfw = false, string flair = null)
        {
            // Trim new title to meet Reddit's 300 character limit
            title = title.Substring(0, Math.Min(title.Length, 299));
            Post post = _client.Subreddit(subreddit).LinkPost(title: title, url: link).Submit(true);
            post.SetFlair(flair);
            if (nsfw)
            {
                post.MarkNSFW();
            }
            return post;
        }

        public async Task<Post> SubmitLinkPostAsync(string title, string link, string subreddit, bool nsfw = false, string flair = null)
        {
            // Trim new title to meet Reddit's 300 character limit
            title = title.Substring(0, Math.Min(title.Length, 299));
            Post post = await _client.Subreddit(subreddit).LinkPost(title: title, url: link).SubmitAsync(true);
            post.SetFlair(flair);
            if (nsfw)
            {
                post.MarkNSFW();
            }
            return post;
        }

        public Comment SubmitComment(Post post, string commentBody)
        {
            return post.Comment(commentBody).Submit();
        }

        public Task<Comment> SubmitCommentAsync(Post post, string commentBody)
        {
            return post.Comment(commentBody).SubmitAsync();
        }

        public bool ModeratesSubreddit(string subreddit)
        {
            return _client.Account.Me.ModeratedSubreddits.Any(i => i.SR == subreddit);
        }
    }
}
