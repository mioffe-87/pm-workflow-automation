using Newtonsoft.Json;

namespace Csp.Api.Model.GoogleApis
{
    public class GoogleAccessToken
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")] //seconds
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }

        //A long-lived token that can be exchanged for a new access token when an access token expires.
        //The Google Auth server issued Refresh tokens never expire — that's the whole point of the refresh tokens. 
        //The refresh token will expire when the user revokes access to your application.
        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class GoogleUserInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string AcctId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "given_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "family_name")]
        public string LastName { get; set; }

        public GoogleAccessToken AccessToken { get; set; }
    }

    public class GoogleApplicationCredentials
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "project_id")]
        public string ProjectId { get; set; }

        [JsonProperty(PropertyName = "private_key_id")]
        public string PrivateKeyId { get; set; }

        [JsonProperty(PropertyName = "private_key")]
        public string PrivateKey { get; set; }

        [JsonProperty(PropertyName = "client_email")]
        public string ClientEmail { get; set; }

        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }
    }
}
