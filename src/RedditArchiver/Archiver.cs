using Microsoft.Extensions.Options;
using Reddit.Controllers;
using Reddit.Exceptions;
using RedditArchiver.Data;
using RedditArchiver.Models;
using RedditArchiver.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RedditArchiver
{
    public class Archiver
    {
        private readonly IDataStore _database;
        private readonly IUserAccount _userAccount;
        private readonly IBotAccount _botAccount;
        private readonly RedditSettings _settings;

        public Archiver(IDataStore dataStore, IUserAccount userAccount, IOptions<RedditSettings> settings)
            : this(dataStore, userAccount, null, settings) { }

        public Archiver(IDataStore dataStore, IUserAccount userAccount, IBotAccount botAccount, IOptions<RedditSettings> settings)
        {
            _database = dataStore;
            _userAccount = userAccount;
            _botAccount = botAccount;
            _settings = settings.Value;
        }

        public async Task Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Post> posts = await GetPostsFromReddit();
            stopwatch.Stop();
            Console.WriteLine($"Got {posts.Count} newly saved posts from Reddit API. Took {stopwatch.Elapsed.TotalSeconds} seconds.");
            stopwatch.Restart();

            await ArchivePosts(posts);
        }

        private async Task<List<Post>> GetPostsFromReddit()
        {
            PostDTO latestSaved = await _database.GetLatestSavedPostAsync();
            List<Post> posts = new List<Post>();
            try
            {
                if (latestSaved != null)
                {
                    posts = _userAccount.GetSavedPostsBefore(latestSaved.Fullname);
                }
                else
                {
                    posts = _userAccount.GetAllSavedPosts();
                }
            }
            catch (Exception ex) when (ex is RedditUnauthorizedException || ex is RedditForbiddenException)
            {
                Console.WriteLine("Got unauthorised or forbidden response from Reddit when attempting to retrieve saved posts. " +
                    "Check that the app has been authorised to access your account (reddit.com/prefs/apps/) and " +
                    "that your user credentials in appsettings.json are correct.");

                bool validInput = false;
                while (!validInput)
                {
                    Console.Write("Run user-account credential setup? (y/n)");
                    ConsoleKeyInfo key = Console.ReadKey();
                    Console.WriteLine();
                    if (key.Key == ConsoleKey.N)
                    {
                        Console.WriteLine("Valid account credentials must be supplied to archive saved posts.");
                        Environment.Exit(1);
                    }
                    else if (key.Key == ConsoleKey.Y)
                    {
                        validInput = true;
                        string scope = "identity,history";
                        if (_settings.Crosspost.UseUserAccount)
                        {
                            scope += ",submit,read,flair";
                        }
                        await GetAccountCredentials(scope);
                        Console.WriteLine("Copy and paste these tokens into your appsettings.json UserCredentials.");
                        Console.WriteLine("Restart the program for the changes to take effect.\n");
                    }
                }
            }
            posts.Reverse();
            return posts;
        }

        private async Task ArchivePosts(List<Post> posts)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (_botAccount != null)
            {
                string crosspostSub = _settings.Crosspost.CrosspostSubreddit;
                if (String.IsNullOrWhiteSpace(crosspostSub))
                {
                    Console.WriteLine("Crosspost subreddit must not be empty when crossposting is enabled.\n" +
                        "Please set the CrosspostSubreddit value in your appsettings.json.");
                    Environment.Exit(1);
                }

                try
                {
                    if (_botAccount.ModeratesSubreddit(crosspostSub))
                    {
                        foreach (var post in posts)
                        {
                            List<Task> tasks = new List<Task>();
                            tasks.Add(_botAccount.SubmitCrosspostAsync(post, crosspostSub));
                            tasks.Add(_database.SavePostAsync(post));
                            await Task.WhenAll(tasks);
                            Console.WriteLine($"\"{post.Title}\" saved to database and crossposted to /r/{_settings.Crosspost.CrosspostSubreddit}");
                        }
                        stopwatch.Stop();
                        Console.WriteLine($"\nSaved and crossposted {posts.Count} posts in {stopwatch.Elapsed.TotalSeconds:N0} seconds");
                    }
                    else
                    {
                        Console.WriteLine("Bot account must be a moderator of the crosspost subreddit.");
                    }
                }
                catch (Exception ex) when (ex is RedditUnauthorizedException || ex is RedditForbiddenException)
                {
                    Console.WriteLine("Got unauthorised or forbidden response from Reddit when attempting to crosspost.");
                    bool useUserAccount = _settings.Crosspost.UseUserAccount;
                    if (useUserAccount)
                    {
                        Console.WriteLine("If you have recently changed your settings to crosspost using your main account, " +
                            "you will need to allow new permissions to your main account.");
                    }
                    else
                    {
                        Console.WriteLine("Check that the app has been authorised to access the bot account (reddit.com/prefs/apps/) and " +
                            "that the bot credentials in appsettings.json are correct.");
                    }
                    bool validInput = false;
                    while (!validInput)
                    {
                        if (useUserAccount)
                        {
                            Console.Write("Rerun user-account credential setup? (y/n)");
                        }
                        else
                        {
                            Console.Write("Run bot-account credential setup? (y/n)");
                        }
                        ConsoleKeyInfo key = Console.ReadKey();
                        Console.WriteLine();
                        if (key.Key == ConsoleKey.N)
                        {
                            Console.WriteLine("Valid account credentials must be supplied to crosspost saved posts.");
                            Environment.Exit(1);
                        }
                        else if (key.Key == ConsoleKey.Y)
                        {
                            validInput = true;
                            string scope = "identity,submit,read,flair";
                            if (useUserAccount)
                            {
                                scope += ",history";
                                await GetAccountCredentials(scope);
                                Console.WriteLine("Copy and paste these tokens into your appsettings.json UserCredentials.");
                            }
                            else
                            {
                                Console.WriteLine("NOTE: Make sure you are signed into Reddit using the account you want to crosspost from before opening the authorisation URL!");
                                await GetAccountCredentials(scope);
                                Console.WriteLine("Copy and paste these tokens into your appsettings.json BotCredentials.");
                            }
                            Console.WriteLine("Restart the program for the changes to take effect.");
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey(true);
                            Environment.Exit(2);
                        }
                    }
                }
            }
            else
            {
                await _database.SavePostsAsync(posts, reverse: false);
                stopwatch.Stop();
                Console.WriteLine($"\nSaved {posts.Count} posts to the database in {stopwatch.Elapsed.TotalSeconds:N0} seconds");
            }
        }

        private async Task GetAccountCredentials(string scope)
        {
            using (var waitHandle = new AutoResetEvent(false))
            {
                Action<Credentials> callbackAction = delegate (Credentials credentials)
                {
                    Console.WriteLine($"Access token: {credentials.AccessToken}");
                    Console.WriteLine($"Refresh token: {credentials.RefreshToken}");
                    waitHandle.Set();
                };

                Console.Write("Enter the App ID you created at reddit.com/prefs/apps/ : ");
                string appId = Console.ReadLine();

                using (var auth = new OAuthTokenService(scope, appId, callbackAction))
                {
                    Console.WriteLine($"Please open the following URL in your browser and click \"Allow\"");
                    Console.WriteLine(auth.AuthorisationUrl);
                    waitHandle.WaitOne();
                    // Wait a bit so the web server can respond with the HTML before it is disposed
                    await Task.Delay(100);
                }
            }
        }
    }
}
