using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Json;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Csp.Api.HelperClasses;
using System.Text;
using Csp.Api.Model.GoogleApis;
using System.Configuration;

namespace Csp.Api.Logic
{
    /**
     * Google IAP (Identity-Aware Proxy) Client
    **/
    public class IAPClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iapClientId">The client id observed on
        /// https://console.cloud.google.com/apis/credentials.</param>
        /// <returns>The OIDC token as a string.</returns>
        public static string CreateOIDCToken(string url)
        {
            string credentialsFilePath = ConfigurationManager.AppSettings["GoogleApplicationCredentials"];

            ServiceAccountCredential saCredential;
            // Read credentials from the credentials .json file.
            if (credentialsFilePath != null && File.Exists(credentialsFilePath.ToString()))
            {
                var creds = File.ReadAllText(credentialsFilePath.ToString(), Encoding.UTF8);
            
                byte[] byteArray = Encoding.UTF8.GetBytes(creds);
                using (var stream = new MemoryStream(byteArray))
                {
                    saCredential = ServiceAccountCredential.FromServiceAccountData(stream);
                }
            }
            else
            {
                return "";
            }

            // Generate a JWT signed with the service account's private key
            // containing a special "target_audience" claim.
            var jwtBasedAccessToken =
                CreateAccessToken(saCredential, url);

            // Request an OIDC token for the Cloud IAP-secured client ID.
            var req = new GoogleAssertionTokenRequest()
            {
                Assertion = jwtBasedAccessToken
            };
            var result = req.ExecuteAsync(saCredential.HttpClient,
                saCredential.TokenServerUrl, CancellationToken.None,
                saCredential.Clock).Result;
            string token = result.IdToken;

            return token;
        }

        /// <summary>
        /// Generate a JWT signed with the service account's private key
        /// containing a special "target_audience" claim.
        /// </summary>
        /// <param name="privateKey">The private key string pulled from
        /// a credentials .json file.</param>
        /// <param name="iapClientId">The client id observed on
        /// https://console.cloud.google.com/apis/credentials.</param>
        /// <param name="email">The e-mail address associated with the
        /// privateKey.</param>
        /// <returns>An access token.</returns>
        static string CreateAccessToken(ServiceAccountCredential saCredential, string iapClientId)
        {
            var now = saCredential.Clock.UtcNow;
            var currentTime = ApiCommon.ToUnixEpochDate(now);
            var expTime = ApiCommon.ToUnixEpochDate(now.AddHours(1));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Aud,
                    GoogleAuthConsts.OidcTokenUrl),
                new Claim(JwtRegisteredClaimNames.Sub, saCredential.Id),
                new Claim(JwtRegisteredClaimNames.Iat, currentTime.ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, expTime.ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, saCredential.Id),

                // We need to generate a JWT signed with the service account's
                // private key containing a special "target_audience" claim.
                // That claim should contain the clientId of IAP we eventually
                // want to access.
                new Claim("target_audience", iapClientId)
            };

            // Encryption algorithm must be RSA SHA-256, according to
            // https://developers.google.com/identity/protocols/OAuth2ServiceAccount
            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(saCredential.Key),
                SecurityAlgorithms.RsaSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
