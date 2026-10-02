using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class JwtTokenResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
