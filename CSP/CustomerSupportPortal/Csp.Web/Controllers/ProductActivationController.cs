using System;
using System.Linq;
using System.Security;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Kendo.Mvc.UI;
using Csp.Web.Security.Attributes;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using Csp.Api.Model.Enums;
using Csp.Web.Filters;
using Kendo.Mvc.Extensions;

namespace Csp.Web.Controllers
{
    [JwtTokenAuthentication]
    public class ProductActivationController : ApiController
    {
        private readonly ISearchAllLogic _searchAllLogic;
        private readonly IProductActivationPageService _productActivationPageService;
        private readonly ISecurityLogic _securityLogic;
        public ProductActivationController(ISearchAllLogic searchAllLogic, IProductActivationPageService productActivationPageService, ISecurityLogic securityLogic)
        {
            _searchAllLogic = searchAllLogic;
            _productActivationPageService = productActivationPageService;
            _securityLogic = securityLogic;
        }



        [HttpGet]
        public IHttpActionResult GetProductActivationData(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            var data = _productActivationPageService.GetActivationPageData(supportAccountId);
            var productActivationDto = new ProductActivationDto
            {
                Activated = data.Activated,
                ReadyForActivation = data.ReadyForActivation,
                TotalReadyForActivation = data.ReadyForActivation.Count,
                TotalActivated = data.Activated.Count
            };

            return Ok(productActivationDto);
        }

        [HttpGet]
        public IHttpActionResult GetProductActivationDetails(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _productActivationPageService.GetProductActivationPageData(supportAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

    }
}