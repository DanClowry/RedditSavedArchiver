using Reddit;
using RedditArchiver.Models;
using static RedditArchiver.Models.RedditSettings;

namespace RedditArchiver.Services
{
    public abstract class BaseAccount
    {
        protected RedditClient _client;

        public BaseAccount(RedditSettings options, Credentials credentials)
        {
            _client = new RedditClient(appId: credentials.AppID,
                accessToken: credentials.AccessToken,
                refreshToken: credentials.RefreshToken,
                userAgent: options.UserAgent);
        }
    }
}
