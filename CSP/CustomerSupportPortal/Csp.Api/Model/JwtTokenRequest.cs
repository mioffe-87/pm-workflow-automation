using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class JwtTokenRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("samlSessionId")]
        public string SessionId { get; set; }
    }

    public class JwtRefreshTokenRequestBase
    {
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }

    public class JwtRefreshTokenRequest : JwtRefreshTokenRequestBase
    {
        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }
    }

    public class JwtValidateTokenRequest
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
}
