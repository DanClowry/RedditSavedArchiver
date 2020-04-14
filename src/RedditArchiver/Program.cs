using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedditArchiver.Data;
using RedditArchiver.Models;
using RedditArchiver.Services;
using System;
using System.Threading.Tasks;

namespace RedditArchiver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Load config
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables("REDDIT_ARCHIVER_")
                .AddCommandLine(args)
                .Build();

            // Configure services
            IServiceProvider services = new ServiceCollection()
                .Configure<RedditSettings>(o => config.Bind("Reddit", o))
                .Configure<ConnectionStrings>(o => config.Bind("ConnectionStrings", o))
                .AddTransient<IDataStore, SqlLiteDataStore>()
                .AddTransient<IAccount, Account>()
                .AddSingleton<Archiver>()
                .BuildServiceProvider();

            // Run program
            await services.GetRequiredService<Archiver>().Run();
        }
    }
}
