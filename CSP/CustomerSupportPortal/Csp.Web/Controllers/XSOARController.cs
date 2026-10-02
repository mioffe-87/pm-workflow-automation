using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Xsoar;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    [Route("api/[controller]")]
    public class XSOARController : ApiController
    {
        private readonly IXSOARLogic _xsoarLogic;

        public XSOARController(IXSOARLogic xsoarLogic)
        {
            _xsoarLogic = xsoarLogic;
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetProductDetails(long supportAccountId, long UserAccountId, string status)
        {
            if (!JwtTokenManager.IsAuthorized(UserAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<XSOARListItem>();

            try
            {
                returnData = _xsoarLogic.GetXSOARProductDetails(supportAccountId, status).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetMarketplaceDetails(long supportAccountId, long UserAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(UserAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                return this.Json(_xsoarLogic.GetMarketplaceDetails(supportAccountId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(new XsoarMarketplacePurchaseDataModel[0]);
            }
        }
    }
}
