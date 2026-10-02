using Csp.Api.Logic;
using Csp.Api.Model.Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    public class CommonApiController : ApiController
    {
        private readonly ICommonApiLogic _commonApiLogic;

        public CommonApiController(ICommonApiLogic commonApiLogic)
        {
            _commonApiLogic = commonApiLogic;
        }



        [HttpPost]

        public IHttpActionResult ValidateAddress([FromBody] AddressAPIRequest addressAPIRequest)
        {
            try
            {
                var result = _commonApiLogic.ValidateAddress(addressAPIRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        public IHttpActionResult IsEligibleFor4HREsa([FromBody] AddressAPIResponse request)
        {
            try
            {
                var result = _commonApiLogic.IsEligibleFor4HREsa(request);
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

