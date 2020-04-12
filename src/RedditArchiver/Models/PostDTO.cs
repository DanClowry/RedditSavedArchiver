using Reddit.Controllers;
using SQLite;
using System;

namespace RedditArchiver.Models
{
    [Table("Posts")]
    public class PostDTO
    {
        public PostDTO() { }

        public PostDTO(Post post)
        {
            Author = post.Author;
            Created = post.Created;
            DownVotes = post.DownVotes;
            Edited = post.Edited;
            Fullname = post.Fullname;
            IsDownvoted = post.IsDownvoted;
            IsUpvoted = post.IsUpvoted;
            NSFW = post.NSFW;
            Permalink = post.Permalink;
            Removed = post.Removed;
            Score = post.Score;
            Spam = post.Spam;
            Subreddit = post.Subreddit;
            Title = post.Title;
            URL = post.Listing.URL;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Author { get; set; }
        public DateTime Created { get; set; }
        public int DownVotes { get; set; }
        public DateTime Edited { get; set; }
        [Indexed]
        public string Fullname { get; set; }
        public bool IsDownvoted { get; set; }
        public bool IsUpvoted { get; set; }
        public bool NSFW { get; set; }
        public string Permalink { get; set; }
        public bool Removed { get; set; }
        public int Score { get; set; }
        public bool Spam { get; set; }
        public string Subreddit { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
    }
}
