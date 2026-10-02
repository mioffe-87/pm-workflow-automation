using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    [Route("api/[controller]")]
    public class CoveoSearchTokenController : ApiController
    {
        private readonly IApplicationSettings _applicationSettings;
        private static readonly TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(50);
        private const string VisibilityFilter = "NOT @sfisvisibleincsp==false";

        public CoveoSearchTokenController(IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        // POST api/coveosearchtoken
        public HttpResponseMessage Post([FromBody] CoveoConfigModel config)
        {
            try
            {
                if (config == null)
                {
                    return CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request.");
                }

                var token = GetOrCreateToken(config);
                return CreateJsonResponse(token);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return CreateErrorResponse(HttpStatusCode.InternalServerError, "An error occurred processing the token request.");
            }
        }

        private string GetOrCreateToken(CoveoConfigModel config)
        {
            bool isAnonymous = config.SupportAccountId <= 0 
                || string.IsNullOrEmpty(config.EmailAddress);

            string userEmail = isAnonymous
                ? ApiConstants.PANW_COVEO_ANONYMOUS_USER_EMAIL
                : config.EmailAddress;

            string cacheKey = $"{userEmail}_CoveoToken";

            if (HttpRuntime.Cache.Get(cacheKey) is string cachedToken)
            {
                return cachedToken;
            }

            string apiKey = isAnonymous
                ? _applicationSettings.CoveoUnauthApiKey
                : _applicationSettings.CoveoApiKey;

            string token = RequestTokenFromCoveo(userEmail, apiKey, isAnonymous);

            if (!string.IsNullOrEmpty(token))
            {
                HttpRuntime.Cache.Insert(cacheKey, token, null,
                    DateTime.Now.Add(TokenCacheDuration), Cache.NoSlidingExpiration);
            }

            return token;
        }

        private string GetCoveoSearchFilter(bool isAnonymous)
        {
            string allowedSources = isAnonymous
                ? _applicationSettings.CoveoSearchAnonymousDataSource
                : _applicationSettings.CoveoSearchDataSource;

            return string.IsNullOrEmpty(allowedSources)
                ? VisibilityFilter
                : $"{VisibilityFilter} AND {allowedSources}";
        }

        private string RequestTokenFromCoveo(string emailAddress, string apiKey, bool isAnonymous)
        {
            string endpointUrl = $"{_applicationSettings.CoveoSearchRestEndpointUrl}/token?organizationId={_applicationSettings.CoveoOrganizationId}";

            string jsonRequest = JsonConvert.SerializeObject(new
            {
                userIds = new[] { new { name = emailAddress, provider = "Email Security Provider" } },
                filter = GetCoveoSearchFilter(isAnonymous),
                searchHub = _applicationSettings.CoveoSearchHub
            });

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var request = (HttpWebRequest)WebRequest.Create(endpointUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Authorization"] = $"Bearer {apiKey}";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(jsonRequest);
            }

            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string jsonResponse = reader.ReadToEnd();
                dynamic tokenResponse = JsonConvert.DeserializeObject(jsonResponse,
                    new JsonSerializerSettings { MaxDepth = 32, TypeNameHandling = TypeNameHandling.None });

                return tokenResponse?.token;
            }
        }

        private static HttpResponseMessage CreateJsonResponse(string content)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(content ?? string.Empty, Encoding.UTF8, "application/json")
            };
        }

        private static HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(new { error = message }),
                    Encoding.UTF8, "application/json")
            };
        }
    }
}