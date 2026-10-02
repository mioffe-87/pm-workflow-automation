using Csp.Api.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Api.Model;
using Csp.Api.Model.Csp.Api.Model;

namespace Csp.Web.Controllers
{
    public class AddressValidationController : ApiController
    {
        private readonly IAddressValidationService _addressValidationService;
        private readonly ISecurityLogic _securityLogic;
        public AddressValidationController(IAddressValidationService addressValidationService, ISecurityLogic securityLogic)
        {
            _addressValidationService = addressValidationService;
            _securityLogic = securityLogic;

        }
       
        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult ValidateAddress([FromBody]AddressAPIRequest requestdata, long supportAccountId, long userAccountId)
        {          
            
            try
            {
                return this.Json(_addressValidationService.ValidateAddress(requestdata));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(new AddressAPIResponse[0]);
            }
        }

        [HttpGet, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult IsAddressValidationRequiredForCountry(int countryId)
        {

            try
            {
                return this.Json(_addressValidationService.IsAddressValidationRequiredForCountry(countryId));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(new AddressAPIResponse[0]);
            }
        }
    }
}
