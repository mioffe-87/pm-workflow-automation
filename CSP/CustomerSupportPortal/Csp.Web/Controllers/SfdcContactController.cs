using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using System;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    public class SfdcContactController : ApiController
    {
        private readonly ISfdcContactLogic _sfdcContactLogic;
        private readonly IApplicationSettings _applicationSettings;

        public SfdcContactController(ISfdcContactLogic sfdcContactLogic, IApplicationSettings applicationSettings)
        {
            _sfdcContactLogic = sfdcContactLogic;
            _applicationSettings = applicationSettings;
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult SfdcCaseCreation(SupportCaseNavigationRequest request)
        {
            dynamic result;
            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                result = _sfdcContactLogic.ManageSfdcCaseCreationHistory(request);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            result = new CommonApiResponse<SupportCaseNavigationResponse>()
            {
                ResponseObject = new SupportCaseNavigationResponse()
                {
                    Link = string.Format(ApiConstants.LoginHelp, _applicationSettings.ApplicationUrl),
                    Message = string.Empty
                }
            };
            return this.Json(result);
        }
    }
}