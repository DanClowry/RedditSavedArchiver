using Reddit.Controllers;
using RedditArchiver.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedditArchiver.Data
{
    public interface IDataStore
    {
        /// <summary>
        /// Gets all saved posts from the database
        /// </summary>
        /// <returns>A list of saved posts in the database</returns>
        Task<List<PostDTO>> GetSavedPostsAsync();

        /// <summary>
        /// Gets the most recently saved post in the database
        /// </summary>
        /// <remarks>Selects the row from the database with the highest ID column value</remarks>
        /// <returns>The most recently saved post or null if there are no posts</returns>
        Task<PostDTO> GetLatestSavedPostAsync();

        /// <summary>
        /// Saves a single post to the database
        /// </summary>
        /// <param name="post">The post to save</param>
        Task SavePostAsync(Post post);

        /// <summary>
        /// Saves a single post to the database
        /// </summary>
        /// <param name="post">The post to save</param>
        Task SavePostAsync(PostDTO post);

        /// <summary>
        /// Saves a list of posts to the database
        /// </summary>
        /// <param name="posts">The list of posts to be saved</param>
        /// <param name="reverse">Whether the list should be reversed before it is saved</param>
        Task SavePostsAsync(List<Post> posts, bool reverse = true);

        /// <summary>
        /// Saves a list of posts to the database
        /// </summary>
        /// <param name="posts">The list of posts to be saved</param>
        /// <param name="reverse">Whether the list should be reversed before it is saved</param>
        Task SavePostsAsync(List<PostDTO> posts, bool reverse = true);
    }
}
