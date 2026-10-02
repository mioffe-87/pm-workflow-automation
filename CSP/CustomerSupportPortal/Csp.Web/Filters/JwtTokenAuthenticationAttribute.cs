using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Csp.Web.Extensions;
using Csp.Web.HelperClasses;
using System.Configuration;
using System.Web;
using Csp.Api.HelperClasses;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Csp.Web.Filters
{
    public class JwtTokenAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public string Realm { get; set; }
        public bool AllowMultiple { get; set; }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;

            if (authorization == null || authorization.Scheme != "Bearer")
            {
                context.ErrorResult = new AuthenticationFailureResult("Unauthorized Access", request);
                return;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing Jwt Token", request);
                return;
            }

            var token = authorization.Parameter;
            try
            {
                var principal = await AuthenticateJwtToken(token);
                if (principal == null)
                    context.ErrorResult = new AuthenticationFailureResult(Constants.InvalidToken, request);
                else
                    context.Principal = principal;
            }
            catch (WebException ex)
            {
                var result = GetStringWebException(ex);
                context.ErrorResult = new AuthenticationFailureResult(Constants.InvalidToken, request, JsonConvert.SerializeObject(result));
            }
        }

        private static bool ValidateToken(string token, out string username)
        {
            username = null;
            try
            {
                username = JwtTokenManager.ValidateJwtToken(token);

                return string.IsNullOrEmpty(username) ? false : true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            string username;

            if (ValidateToken(token, out username))
            {
                // based on username to get more information from database in order to build local identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                    // Add more claims if needed: Roles, ...
                };

                var identity = new ClaimsIdentity(claims, "Jwt");
                IPrincipal user = new ClaimsPrincipal(identity);

                return Task.FromResult(user);
            }

            return Task.FromResult<IPrincipal>(null);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter = null;

            if (!string.IsNullOrEmpty(Realm))
                parameter = "realm=\"" + Realm + "\"";

            context.ChallengeWith("Bearer", parameter);
        }

        private dynamic GetStringWebException(WebException ex)
        {
            string response = string.Empty;

            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
            {
                string jsonResponse = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    response = jsonResponse.ToString();
                }
            }
            return JsonConvert.DeserializeObject(response);
        }
    }

    public class AuthenticationFailureResult : IHttpActionResult
    {
        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request, string errorResponse = "")
        {
            ReasonPhrase = reasonPhrase;
            Request = request;
            ErrorResponse = errorResponse;
        }

        public string ReasonPhrase { get; set; }

        public string ErrorResponse { get; set; }

        public HttpRequestMessage Request { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = Request,
                ReasonPhrase = ReasonPhrase,
                Content = new StringContent(ErrorResponse, Encoding.UTF8, "application/json")
            };

            return response;
        }
    }
}