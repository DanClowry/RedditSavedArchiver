using EmbedIO;
using EmbedIO.Actions;
using RedditArchiver.Models;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedditArchiver.Services
{
    class OAuthTokenService : IDisposable
    {
        private string _appId;
        private string _url = "http://localhost:9696/";
        private string _redirectUrl
        {
            get
            {
                return $"{_url}redditArchiver/callback";
            }
        }
        private string _scope;
        private Action<Credentials> _completedAuthCallback;
        private readonly string _state = Guid.NewGuid().ToString("N");

        public string AuthorisationUrl
        {
            get
            {
                return $"https://www.reddit.com/api/v1/authorize?client_id={_appId}&response_type=code&" +
                    $"state={_state}&redirect_uri={_redirectUrl}&duration=permanent&scope={_scope}";
            }
        }

        private WebServer _webServer;
        private MemoryStream _memoryStream;
        private TextWriter _textWriter;
        private static readonly HttpClient _httpClient = new HttpClient();

        public Credentials Credentials { get; private set; }

        public OAuthTokenService(string scope, string appId, Action<Credentials> completedAuthCallback)
        {
            _appId = appId;
            _scope = scope;
            _completedAuthCallback = completedAuthCallback;
            Logger.NoLogging();
            _webServer = CreateWebServer();
            _webServer.RunAsync();
        }

        private WebServer CreateWebServer()
        {
            _memoryStream = new MemoryStream();
            _textWriter = new StreamWriter(_memoryStream);
            Action<TextWriter> htmlWriter = delegate (TextWriter textWriter)
            {
                textWriter.WriteLine($"<p>Access token: {Credentials.AccessToken}</p>");
                textWriter.WriteLine($"<p>Refresh token: {Credentials.RefreshToken}</p>");
            };
            return new WebServer(o => o
                    .WithUrlPrefix(_url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithModule(new ActionModule("/redditArchiver/callback", HttpVerbs.Any, ctx =>
                {
                    Credentials = RetrieveToken(ctx.GetRequestQueryData()).Result;
                    _completedAuthCallback(Credentials);
                    return ctx.SendStandardHtmlAsync(200, htmlWriter);
                }));
        }

        private async Task<Credentials> RetrieveToken(NameValueCollection queryData)
        {
            if (!string.IsNullOrWhiteSpace(queryData["error"]))
            {
                throw new Exception($"Reddit returned error regarding authorisation. Error value: {queryData["error"]}");
            }

            if (queryData["state"] != _state)
            {
                throw new Exception($"State returned by Reddit does not match state sent.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_appId}:")));
            string code = queryData["code"];
            var tokenRequestData = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectUrl }
            };
            HttpResponseMessage tokenResponse = await _httpClient.PostAsync("https://www.reddit.com/api/v1/access_token", new FormUrlEncodedContent(tokenRequestData));
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new Exception("Reddit returned non-success status code when getting access token.");
            }
            string tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            if (tokenResponseContent.Contains("error"))
            {
                throw new Exception($"Reddit returned error when getting access token. JSON response: {tokenResponseContent}");
            }
            var credentials = JsonSerializer.Deserialize<Credentials>(tokenResponseContent);
            credentials.AppID = _appId;
            return credentials;
        }

        public void Dispose()
        {
            _webServer.Dispose();
            _textWriter.Dispose();
            _memoryStream.Dispose();
        }
    }
}
