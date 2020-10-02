using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShudderScrobbler.Shudder.Model
{
    public class LoginRequestModel
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
