using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            IServiceCollection serviceCollection = new ServiceCollection()
                .Configure<RedditSettings>(o => config.Bind("Reddit", o))
                .Configure<ConnectionStrings>(o => config.Bind("ConnectionStrings", o))
                .Configure<Credentials>(o => config.Bind("Reddit:UserCredentials", o))
                .AddTransient<IDataStore, SqlLiteDataStore>()
                .AddTransient<IUserAccount, UserAccount>()
                .AddSingleton<Archiver>();

            // Add crossposting account to services if setting is enabled
            if (config.GetValue<bool>("Reddit:Crosspost:EnableCrossposting") == true)
            {
                serviceCollection.Configure<Credentials>("BotCredentials",
                    o => config.Bind("Reddit:Crosspost:BotCredentials", o));

                serviceCollection.AddTransient<IBotAccount, BotAccount>(r =>
                {
                    Credentials credentials = config.GetValue<bool>("Reddit:Crosspost:UseUserAccount") ?
                        r.GetRequiredService<IOptions<Credentials>>().Value :
                        r.GetRequiredService<IOptionsSnapshot<Credentials>>().Get("BotCredentials");
                    return new BotAccount(r.GetRequiredService<IOptionsSnapshot<RedditSettings>>(),
                        credentials);
                });
            }

            IServiceProvider services = serviceCollection.BuildServiceProvider();

            // Run program
            await services.GetRequiredService<Archiver>().Run();
        }
    }
}
