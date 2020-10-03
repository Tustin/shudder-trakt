using Newtonsoft.Json;
using ShudderScrobbler.Shudder.Model;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShudderScrobbler.Shudder
{
    public class ShudderClient
    {
        private string Username { get; set; }
        private string Password { get; set; }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }

        private HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            Proxy = new WebProxy("localhost", 8888)
        });

        public ShudderClient()
        {
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-DEVICE-ID", Guid.NewGuid().ToString());
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-SERVICE-NAME", "shudder");
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-PLATFORM-NAME", "ios");
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-SERVER-ENV", "prod");

            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Shudder/3.2.20 (com.sundancenow.shudder; build:0; iOS 13.5.0) Alamofire/4.7.3");
        }

        public async Task LoginAsync(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            var deviceTokenRequest = await this._httpClient.PostAsync("https://devices.amcsvod.io/v1/register", default);
            if (!deviceTokenRequest.IsSuccessStatusCode)
            {
                throw new Exception("Invalid registration"); // TODO
            }

            if (!deviceTokenRequest.Headers.TryGetValues("X-API-DEVICE-TOKEN", out var values))
            {
                throw new Exception("Missing device token");
            }

            var deviceToken = values.FirstOrDefault();

            if (deviceToken == default)
            {
                throw new Exception("Missing device token");
            }

            // Need this to login
            this._httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", deviceToken);

            var contents = new StringContent(JsonConvert.SerializeObject(
                new LoginRequestModel()
                {
                    Username = Username,
                    Password = Password
                }), Encoding.UTF8, "application/json");

            var loginRequest = await this._httpClient.PostAsync("https://auth.amcsvod.io/v1/login", contents);

            if (!loginRequest.IsSuccessStatusCode)
            {
                throw new Exception($"Bad status code '{loginRequest.StatusCode}' from login request");
            }

            if (!loginRequest.Headers.TryGetValues("Authorization", out values))
            {
                throw new Exception("Missing device token");
            }

            var accessToken = values.FirstOrDefault();

            if (accessToken == default)
            {
                throw new Exception("Missing access token from login response");
            }

            this.SetAccessToken(accessToken["Bearer ".Length..]);

            if (!loginRequest.Headers.TryGetValues("X-API-REFRESH-TOKEN", out values))
            {
                throw new Exception("Missing refresh token");
            }

            var refreshToken = values.FirstOrDefault();

            if (refreshToken == default)
            {
                throw new Exception("Missing refresh token from login response");
            }

            this.SetRefreshToken(refreshToken);
        }

        public async Task<ContinueWatchingModel> GetWatchingNow()
        {
            var response = await _httpClient.GetAsync("https://continue-watching.amcsvod.io/v2/continuewatching/");

            return JsonConvert.DeserializeObject<ContinueWatchingModel>(await response.Content.ReadAsStringAsync());
        }

        public async Task<MovieModel> GetMovie(string id)
        {
            var response = await _httpClient.GetAsync($"https://vmh-api.amcsvod.io/v2/movies/{id}?filters=true");

            return JsonConvert.DeserializeObject<MovieModel>(await response.Content.ReadAsStringAsync());

        }

        public void SetAccessToken(string accessToken)
        {
            this.AccessToken = accessToken;
            this._httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.AccessToken);
        }

        public void SetRefreshToken(string refreshToken)
        {
            this.RefreshToken = refreshToken;
        }

        public async Task<HttpResponseMessage> Get(string endpoint)
        {
            return await this._httpClient.GetAsync(endpoint);
        }

        public async Task<string> GetString(string endpoint)
        {
            return await this.Get(endpoint).Result.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> Post(string endpoint, HttpContent content)
        {
            return await this._httpClient.PostAsync(endpoint, content);
        }
    }
}
