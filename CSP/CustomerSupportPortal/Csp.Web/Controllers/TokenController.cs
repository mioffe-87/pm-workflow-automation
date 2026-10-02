using Csp.Api.Services.Contracts;
using Csp.Web.HelperClasses;
using opentoken;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using Csp.Api.Model;
using System.Net;
using Csp.Api.Model.Enums;
using Csp.Api.HelperClasses;
using Csp.Web.Extensions;

namespace Csp.Web.Controllers
{
    public class TokenController : ApiController
    {
        [HttpPost]
        public IHttpActionResult RefreshJwtToken([FromBody] JwtRefreshTokenRequest jwtRefreshTokenRequest)
        {
            if (!JwtTokenManager.IsAuthorized(jwtRefreshTokenRequest.UserAccountId, jwtRefreshTokenRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var response = JwtTokenManager.RefreshJwtToken(jwtRefreshTokenRequest.RefreshToken);
                return this.Json(response);
            }
            catch (WebException ex)
            {
                var errorMessage = ExceptionHelper.GetWebException<dynamic>(ex);
                return WebExtensions.HttpUnAuthorizedErrorResponse(HttpStatusCode.Unauthorized, errorMessage, this);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }
    }
}
