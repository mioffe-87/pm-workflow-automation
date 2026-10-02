using Csp.Api.Services.Contracts;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using Csp.Api.Model;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Web.Security;
using System.Net;

namespace Csp.Web.HelperClasses
{
    public static class JwtTokenManager
    {
        internal static Func<string, string, string, string, bool> TokenValidationFunc = (baseUrl, method, certThumbPrint, jsonPayload) =>
        {
            string userAgent = GetUserAgent();
            return CommonHelper.ProcessRequest(baseUrl, method, certThumbPrint, jsonPayload, true, userAgentHeader: userAgent);
        };

        public static JwtTokenResponse GenerateToken(string userName)
        {
            JwtTokenResponse jwtTokenResponse = new JwtTokenResponse();
            if (userName == null)
            {
                jwtTokenResponse = new JwtTokenResponse();
            }
            else
            {
                string sessionId = AddSessionId();

                if (string.IsNullOrEmpty(sessionId)) return new JwtTokenResponse();

                JwtTokenRequest request = new JwtTokenRequest
                {
                    Email = userName,
                    SessionId = sessionId
                };
                string refreshToken = GetRefreshToken(ApiConstants.CspRefreshToken);

                jwtTokenResponse = string.IsNullOrEmpty(refreshToken) ?
                    GetJwtToken(request, ApiConstants.JwtTokenEndpoint).ResponseObject : GetNewJwtToken(refreshToken).ResponseObject;

                if (string.IsNullOrEmpty(refreshToken))
                {
                    AddCspRefreshToken(jwtTokenResponse);
                }
            }
            return GetJwtTokenResponse(jwtTokenResponse);
        }

        public static bool IsAuthorized(string user = "", ClaimType claimType = ClaimType.Name)
        {
            try
            {
                var token = (HttpContext.Current != null) ? HttpContext.Current.Request.Headers["Authorization"] : string.Empty;

                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                token = token.Replace("Bearer ", "");

                if (!string.IsNullOrEmpty(user))
                {
                    var jwtToken = ValidateAndReadToken(token);

                    if (jwtToken == null)
                        return false;

                    var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType.ToDescription());

                    if (claim != null && claim.Value == user)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                return false;
            }
        }

        public static bool IsAuthorized(long userAccountId, long supportAccountId, ClaimType claimType = ClaimType.Name)
        {
            try
            {
                var securityLogic = DependencyResolver.Current.GetService<ISecurityLogic>();
                var userAccount = new UserAccount()
                {
                    Id = userAccountId
                };
                var userPermissions = securityLogic.GetSupportAccountPermissionsForPrincipal(
                                    supportAccountId, userAccount);

                return IsAuthorized(userAccountId.ToString(), claimType) && userPermissions.Any();
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                return false;
            }
        }

        public static CommonApiResponse<dynamic> RefreshJwtToken(string refreshToken)
        {
            dynamic result = null;
            try
            {
                string userAgent = GetUserAgent();
                JwtRefreshTokenRequestBase jwtRefreshTokenRequest = new JwtRefreshTokenRequestBase()
                {
                    RefreshToken = refreshToken
                };

                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(jwtRefreshTokenRequest);
                var baseUrl = string.Format(ApiConstants.JwtRefreshTokenEndpoint, ConfigurationManager.AppSettings["CspAuthHost"]);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, ConfigurationManager.AppSettings["CspAuthCertThumbPrint"], jsonPayload, true, userAgentHeader: userAgent);
            }
            catch (WebException ex)
            {
                throw ex;
            }
            return result;
        }

        public static string ValidateJwtToken(string accessToken)
        {
            try
            {
                var jwtToken = ValidateAndReadToken(accessToken);
                return jwtToken?.Claims?.FirstOrDefault(c => c.Type == ClaimType.Name.ToDescription())?.Value?.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static JwtSecurityToken ValidateAndReadToken(string accessToken)
        {
            JwtValidateTokenRequest jwtValidateTokenRequest = new JwtValidateTokenRequest()
            {
                AccessToken = accessToken
            };
            var method = HttpMethod.Post.Method;
            var jsonPayload = JsonConvert.SerializeObject(jwtValidateTokenRequest);
            var baseUrl = string.Format(ApiConstants.JwtTokenValidationEndpoint, ConfigurationManager.AppSettings["CspAuthHost"]);
            var certThumbPrint = ConfigurationManager.AppSettings["CspAuthCertThumbPrint"];
            var response = TokenValidationFunc(baseUrl, method, certThumbPrint, jsonPayload);
            if (response)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;
                return jwtToken;
            }
            return null;
        }

        private static CommonApiResponse<JwtTokenResponse> GetJwtToken(JwtTokenRequest jwtTokenRequest, string endPoint)
        {
            try
            {
                string userAgent = GetUserAgent();
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(jwtTokenRequest);
                var baseUrl = string.Format(endPoint, ConfigurationManager.AppSettings["CspAuthHost"]);
                return CommonHelper.ProcessWebRequest<JwtTokenResponse>(baseUrl, method, ConfigurationManager.AppSettings["CspAuthCertThumbPrint"], jsonPayload, true, userAgentHeader: userAgent);
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                return new CommonApiResponse<JwtTokenResponse>();
            }
        }

        private static string AddSessionId()
        {
            var sessionId = GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null)
                {
                    return string.Empty;
                }
                sessionId = authCookie.Value;
                var sessionCookie = new HttpCookie(ApiConstants.SupportSessionId);
                sessionCookie.HttpOnly = true;
                sessionCookie.Secure = true;
                sessionCookie.Value = sessionId;
                sessionCookie.Expires = DateTime.Now.AddHours(8);
                HttpContext.Current.Response.Cookies.Add(sessionCookie);
            }
            return sessionId;
        }

        private static string GetSessionId()
        {
            var sessionIdCookie = HttpContext.Current != null ? HttpContext.Current.Request.Cookies.Get(ApiConstants.SupportSessionId) : null;

            if (sessionIdCookie != null)
            {
                return sessionIdCookie.Value;
            }
            return string.Empty;
        }

        private static JwtTokenResponse GetJwtTokenResponse(JwtTokenResponse jwtTokenResponse)
        {
            if (string.IsNullOrEmpty(jwtTokenResponse?.AccessToken))
            {
                jwtTokenResponse = new JwtTokenResponse()
                {
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty
                };
            }
            return jwtTokenResponse;
        }

        private static void AddCspRefreshToken(JwtTokenResponse jwtTokenResponse)
        {
            if (!string.IsNullOrEmpty(jwtTokenResponse?.RefreshToken))
            {
                var cspRefreshToken = new HttpCookie(ApiConstants.CspRefreshToken);
                cspRefreshToken.HttpOnly = true;
                cspRefreshToken.Secure = true;
                cspRefreshToken.Value = jwtTokenResponse.RefreshToken;
                cspRefreshToken.Expires = DateTime.Now.AddHours(8);
                HttpContext.Current.Response.Cookies.Add(cspRefreshToken);
            }
        }

        private static string GetRefreshToken(string name)
        {
            var refreshCookie = HttpContext.Current != null ? HttpContext.Current.Request.Cookies.Get(name) : null;
            return refreshCookie != null ? refreshCookie.Value : string.Empty;
        }

        private static CommonApiResponse<JwtTokenResponse> GetNewJwtToken(string refreshToken)
        {
            try
            {
                string userAgent = GetUserAgent();
                JwtRefreshTokenRequestBase jwtRefreshTokenRequest = new JwtRefreshTokenRequestBase()
                {
                    RefreshToken = refreshToken
                };

                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(jwtRefreshTokenRequest);
                var baseUrl = string.Format(ApiConstants.JwtRefreshTokenEndpoint, ConfigurationManager.AppSettings["CspAuthHost"]);
                return CommonHelper.ProcessWebRequest<JwtTokenResponse>(baseUrl, method, ConfigurationManager.AppSettings["CspAuthCertThumbPrint"], jsonPayload, true, userAgentHeader: userAgent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetUserAgent()
        {
            string userAgent = "";

            if (HttpContext.Current.Request.Headers["User-Agent"] != null)
            {
                var headers = HttpContext.Current.Request.Headers.GetValues("User-Agent");

                StringBuilder sb = new StringBuilder();

                foreach (var header in headers)
                {
                    sb.Append(header);
                    sb.Append(" ");
                }
                userAgent = sb.ToString().Trim();
            }
            return userAgent;
        }
    }
}