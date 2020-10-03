using ShudderScrobbler.Shudder;
using ShudderScrobbler.Shudder.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
        private static readonly ShudderClient shudderClient = new ShudderClient();

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
                Console.WriteLine("[err] Failed generating Trakt authorization tokens:", ex.Message);
                return false;
            }
        }

        static async Task<bool> PerformShudderAuthentication()
        {
            Console.Write("Shudder username: ");
            var username = Console.ReadLine();
            Console.Write("Shudder password: ");
            var password = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            Console.WriteLine("Logging in...");

            try
            {
                await shudderClient.LoginAsync(username, password);
                FileService.Instance.Config.Shudder.AccessToken = shudderClient.AccessToken;
                FileService.Instance.Config.Shudder.RefreshToken = shudderClient.RefreshToken;
                FileService.Instance.Write();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[err] Failed generating Shudder authorization tokens:", ex.Message);
                return false;
            }
        }

        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
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
            else
            {
                traktClient.Authorization = TraktAuthorization.CreateWith(
                    FileService.Instance.Config.Trakt.AccessToken,
                    FileService.Instance.Config.Trakt.RefreshToken);
            }

            if (!FileService.Instance.Config.Shudder.HasTokens())
            {
                if (!await PerformShudderAuthentication())
                {
                    Console.WriteLine("Unable to authenticate with Shudder. Please try again later");
                    return;
                }
            }
            else
            {
                shudderClient.SetAccessToken(FileService.Instance.Config.Shudder.AccessToken);
                shudderClient.SetRefreshToken(FileService.Instance.Config.Shudder.RefreshToken);
            }

            Console.Clear();

            Console.WriteLine("Waiting for plays...");

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            ContinueWatching lastWatchedContent = default;
            MovieModel lastWatchedMovie = default;
            while (true)
            {
                var watching = await shudderClient.GetWatchingNow();

                var latestWatching = watching.ContinueWatching.First();

                if (lastWatchedContent != default)
                {
                    if (latestWatching.Type == TypeEnum.Movie)
                    {
                        // Movie changed
                        if (lastWatchedContent.Id != latestWatching.Id)
                        {
                            lastWatchedMovie = await shudderClient.GetMovie(latestWatching.Id);
                            var releaseDate = DateTimeOffset.FromUnixTimeMilliseconds(lastWatchedMovie.Video.PhotoVideoMetadataIptc.DateReleased);
                            var slug = $"{GenerateSlug(lastWatchedMovie.Video.PhotoVideoMetadataIptc.Title[0].Body)}-{releaseDate.Year}";
                            lastWatchedContent = latestWatching;
                            var traktMovie = await traktClient.Movies.GetMovieAsync(slug);
                            // TODO: For multiple results, maybe compare the durations??

                            await traktClient.Scrobble.StartMovieAsync(traktMovie.Value, 0f);
                        }
                        else
                        {
                            // If the movie didn't change, check the timestamp and see if the movie is still being played.
                        }
                    }
      
                    if (!lastWatchedContent.Equals(latestWatching))
                    {
                        var lastPlayedTime = DateTimeOffset.FromUnixTimeSeconds(latestWatching.Timestamp);
                            Console.WriteLine(lastPlayedTime);
                        }
                    }
                }

                Console.WriteLine("Not watching anything");
                await Task.Delay(TimeSpan.FromSeconds(10), token);
            }
        }
    }
}
