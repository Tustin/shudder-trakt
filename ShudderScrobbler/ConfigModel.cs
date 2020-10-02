using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShudderScrobbler
{
    public class TraktConfig
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_on")]
        public DateTime ExpiresOn { get; set; }

        public bool HasTokens()
        {
            return this.AccessToken != default && this.RefreshToken != default;
        }

        public bool IsExpired()
        {
            return DateTime.Now > this.ExpiresOn;
        }
    }

    public class ShudderConfig
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        public bool HasTokens()
        {
            return this.AccessToken != default && this.RefreshToken != default;
        }
        
        // It doesn't appear that Shudder has a property for an expiration time.
    }

    public class ConfigModel
    {
        [JsonProperty("trakt")]
        public TraktConfig Trakt { get; set; }

        [JsonProperty("shudder")]
        public ShudderConfig Shudder { get; set; }
    }
}
