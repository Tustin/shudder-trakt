using Newtonsoft.Json;
using ShudderScrobbler.Shudder.Model;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShudderScrobbler.Shudder
{
    public class Client
    {
        private string Username { get; set; }
        private string Password { get; set; }

        private HttpClient _httpClient = new HttpClient();

        public Client()
        {
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-DEVICE-ID", Guid.NewGuid().ToString());
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-SERVICE-NAME", "shudder");
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-PLATFORM-NAME", "ios");
            this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Shudder/3.2.20 (com.sundancenow.shudder; build:0; iOS 13.5.0) Alamofire/4.7.3");
        }

        public async void LoginAsync()
        {
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
