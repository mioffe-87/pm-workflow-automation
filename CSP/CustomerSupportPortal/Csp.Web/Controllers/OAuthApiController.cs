using System;
using System.Web.Http;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using Csp.Api.HelperClasses;
using System.Collections.Generic;
using Csp.Api.Logic;
using System.Configuration;
using ValidateAntiForgeryToken = System.Web.Mvc.ValidateAntiForgeryTokenAttribute;
using System.Threading.Tasks;

namespace Csp.Web.Controllers
{
    public class OAuthApiController : ApiController
    {

        private readonly IOAuthApiLogic _oAuthApiLogic;       

        public OAuthApiController(IOAuthApiLogic oAuthApiLogic)
        {
            _oAuthApiLogic = oAuthApiLogic;

        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetOAuthCreds(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            
            try
            {
                var response = _oAuthApiLogic.GetOAuthCreds(userAccountId,supportAccountId);
                return this.Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [JwtTokenAuthentication]
        public async Task<IHttpActionResult> CreateRegenerateOAuthCreds([FromBody] OAuthApiRequest request)
        {
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var response = _oAuthApiLogic.CreateRegenerateOAuthCreds(request);
                return this.Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetCustomerDetails(long userAccountId,string clientId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var response = _oAuthApiLogic.GetCustomerDetails(clientId);
                return this.Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }
    }
}
