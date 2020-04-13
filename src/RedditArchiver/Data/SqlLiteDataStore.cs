using Reddit.Controllers;
using RedditArchiver.Models;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditArchiver.Data
{
    class SqlLiteDataStore : IDataStore
    {
        private readonly SQLiteAsyncConnection db;

        public SqlLiteDataStore(ConnectionStrings connectionStrings)
        {
            db = new SQLiteAsyncConnection(connectionStrings.SqliteLocation, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            db.CreateTableAsync<PostDTO>().Wait();
        }

        public async Task<List<PostDTO>> GetSavedPostsAsync()
        {
            return await db.QueryAsync<PostDTO>("SELECT * FROM Posts");
        }

        public async Task<PostDTO> GetLatestSavedPostAsync()
        {
            List<PostDTO> results = await db.QueryAsync<PostDTO>("SELECT * FROM Posts ORDER BY Id DESC LIMIT 1");
            return results.FirstOrDefault();
        }

        public async Task SavePostAsync(Post post)
        {
            await SavePostAsync(new PostDTO(post));
        }

        public async Task SavePostAsync(PostDTO post)
        {
            await db.InsertAsync(post);
        }

        public async Task SavePostsAsync(List<Post> posts, bool reverse = true)
        {
            List<PostDTO> postDTOs = new List<PostDTO>();
            foreach (var post in posts)
            {
                postDTOs.Add(new PostDTO(post));
            }
            await SavePostsAsync(postDTOs, reverse);
        }

        public async Task SavePostsAsync(List<PostDTO> posts, bool reverse = true)
        {
            await db.RunInTransactionAsync(transaction =>
            {
                if (reverse)
                {
                    // Iterate through the list in reverse order so posts are saved
                    // to the database in chronological order
                    for (int i = posts.Count - 1; i >= 0; i--)
                    {
                        transaction.Insert(posts[i]);
                    }
                }
                else
                {
                    foreach (var post in posts)
                    {
                        transaction.Insert(post);
                    }
                }
            });
        }
    }
}
