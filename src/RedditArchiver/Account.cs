using Reddit;
using Reddit.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace RedditArchiver
{
    public class Account
    {
        private RedditClient _client;

        public Account(string appId, string accessToken, string refreshToken, string userAgent)
        {
            _client = new RedditClient(appId: appId, accessToken: accessToken,
                refreshToken: refreshToken, userAgent: userAgent);
        }
    }
}
