using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TraktNet;
using TraktNet.Objects.Authentication;
using TraktNet.Objects.Post.Users.CustomListItems;
using TraktNet.Objects.Post.Users.CustomListItems.Responses;

namespace ShudderScrobbler
{
    class Program
    {
        private static readonly string webServerUrl = "http://localhost:56632/";
        private static readonly WebServer webServer = new WebServer(HandleResponse, webServerUrl);
        private static readonly TraktClient traktClient = new TraktClient(
            Environment.GetEnvironmentVariable("TRAKT_CLIENT_ID"),
            Environment.GetEnvironmentVariable("TRAKT_CLIENT_SECRET")
            );

        private static readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private static ManualResetEvent resetEvent = new ManualResetEvent(false);

        static string HandleResponse(HttpListenerRequest request)
        {
            var query = request.QueryString;
            if (query.Count == 0)
            {
                return default;
            }

            try
            {
                var code = (from key in query.Cast<string>()
                            where key == "code"
                            from value in query.GetValues(key)
                            select value).Single();

                if (code != default)
                {
                    traktClient.Authentication.OAuthAuthorizationCode = code;
                    resetEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[err] Failed parsing code from query string", ex);
            }
            return default;

        }
        static async Task<bool> PerformTraktAuthentication()
        {
            traktClient.Authentication.RedirectUri = webServerUrl;
            webServer.Run();

            var processInfo = new ProcessStartInfo(traktClient.Authentication.CreateAuthorizationUrl())
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(processInfo);
            Console.WriteLine("Waiting for Trakt callback...");
            resetEvent.WaitOne();
            Console.WriteLine("Generating authorization tokens...");

            try
            {
                var tokens = await traktClient.Authentication.GetAuthorizationAsync();

                if (!tokens.IsSuccess)
                {
                    Console.WriteLine("[err] Failed authenticating");
                    return false;
                }

                FileService.Instance.Config.Trakt.AccessToken = traktClient.Authorization.AccessToken;
                FileService.Instance.Config.Trakt.RefreshToken = traktClient.Authorization.RefreshToken;
                FileService.Instance.Config.Trakt.ExpiresOn = traktClient.Authorization.CreatedAt.AddSeconds(traktClient.Authorization.ExpiresInSeconds);
                FileService.Instance.Write();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[err] Failed generating authorization tokens", ex.Message);
                return false;
            }
        }
        static async Task Main(string[] args)
        {
            if (!FileService.Instance.Config.Trakt.HasTokens())
            {
                if (!await PerformTraktAuthentication())
                {
                    Console.WriteLine("Unable to authenticate with Trakt. Please try again later");
                    return;
                }
            }
        }
    }
}
