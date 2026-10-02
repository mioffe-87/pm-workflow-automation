using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using System;
using System.Net;
using System.Web.Http;

namespace Csp.Web.Controllers {
	public class UserRegistrationController : ApiController {

		private readonly IUserAccountApiLogic _userAccountApiLogic;
        private readonly ISupportAccountUserLogic _supportAccountUserLogic;

        public UserRegistrationController(IUserAccountApiLogic userAccountapiLogic, 
            ISupportAccountUserLogic supportAccountUserLogic) {
            _userAccountApiLogic = userAccountapiLogic;
            _supportAccountUserLogic = supportAccountUserLogic;
		}

        [HttpPost]
        public IHttpActionResult ValidateCaptcha([FromBody] dynamic request)
        {
            var result = _supportAccountUserLogic.ValidateCaptcha(request);
            return Json(result);
        }

        [HttpPost]
        public IHttpActionResult ValidatePasswordResetCaptcha([FromBody] dynamic request)
        {
            var result = _supportAccountUserLogic.ValidatePasswordResetCaptcha(request);
            return Json(result);
        }

        [HttpPost]
        public IHttpActionResult ValidateEmail([FromBody] dynamic request) {
            var result = _supportAccountUserLogic.ValidateEmail(request);
            return ContentWithStatusCode(result);
        }

        [HttpPost]
        public IHttpActionResult ValidateSelfRegistrationCode([FromBody] dynamic request)
        {
            var result = _supportAccountUserLogic.ValidateSelfRegistrationCode(request);
            return ContentWithStatusCode(result);
        }


        [HttpPost]
        public IHttpActionResult ValidateProductSku([FromBody] dynamic request)
        {
            var result = _supportAccountUserLogic.ValidateProductSku(request);
            return ContentWithStatusCode(result);
        }

        [HttpPost]
        public IHttpActionResult ValidateClaisec([FromBody] dynamic request)
        {
            var result = _supportAccountUserLogic.ValidateClaisec(request);
            return Json(result);
        }

        [HttpPost]
        public IHttpActionResult ValidateNetsec([FromBody] dynamic request)
        {
            var result = _supportAccountUserLogic.ValidateNetsec(request);
            return Json(result);
        }

        [HttpPost]
        public IHttpActionResult GenerateSelfRegistrationOtp([FromBody] SelfRegistraionOtpRequest generateOtpRequest)
        {
            string result = string.Empty;
            try
            {
               result = _userAccountApiLogic.GenerateSelfRegistrationOtp(generateOtpRequest);              
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(result);
        }

        [HttpPost]
        public IHttpActionResult GeneratePasswordResetOtp([FromBody] SelfRegistraionOtpRequest generateOtpRequest)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.GeneratePasswordResetOtp(generateOtpRequest);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(result);
        }

        [HttpGet]
        public IHttpActionResult IsIdpEnabled([FromUri]string domain)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.IsIdpEnabled(domain);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(result);
        }

        [HttpPost]
        public IHttpActionResult ContactSupport([FromBody]dynamic request)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.ContactSupport(request);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(result);
        }

        [HttpPost]
        public IHttpActionResult ValidateSelfRegistrationOtp([FromBody] SelfRegistraionOtpValidationRequest validateOtpRequest)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.ValidateSelfRegistrationOtp(validateOtpRequest);            
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(result);
        }



        [HttpPost]
        public IHttpActionResult RegisterClaisec(dynamic model)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.RegisterClaisec(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                
            }
            return this.Json(result);
        }

        [HttpPost]
        public IHttpActionResult RegisterNetsec(dynamic model)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.RegisterNetsec(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return this.Json(result);
        }

        [HttpPost]
        public IHttpActionResult RegisterByCode(dynamic model)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.RegisterByCode(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }
            return this.Json(result);
        }


        [HttpGet]
        public IHttpActionResult ValidatePublicDomain(long supportAccountId, string email)
        {
            var result = _supportAccountUserLogic.ValidatePublicDomain(supportAccountId, email);
            return Json(result);
        }

        [HttpGet]
        public IHttpActionResult GetUserData(string email)
        {
            var result = _supportAccountUserLogic.GetOktaSSOData(email);
            return Json(result);
        }

        [HttpGet]
        public IHttpActionResult HandleStatus(string email, string status)
        {
            var result = _supportAccountUserLogic.HandleStatus(email, status);
            return Json(result);
        }

        [HttpGet]
        public IHttpActionResult GetCspMembershipData(string email, long supportAccountId)
        {
            var result = _supportAccountUserLogic.GetCspMembershipData(email, supportAccountId);
            return Json(result);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetEligibleAccounts(string email)
        {
            if (!JwtTokenManager.IsAuthorized(email))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetEligibleAccountsByUserEmail(email);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        public IHttpActionResult RemoveBounceListEmails ([FromBody]EmailList request)
        {
            dynamic result = null;
            try
            {
                result = _userAccountApiLogic.RemoveBounceListEmails(request);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(result);
        }

        private IHttpActionResult ContentWithStatusCode(CommonApiResponse<dynamic> result)
        {
            var statusCode = (int)HttpStatusCode.OK;
            try
            {
                var responseObject = result?.ResponseObject;
                if (responseObject != null)
                {
                    var jToken = responseObject as Newtonsoft.Json.Linq.JToken;
                    if (jToken != null && jToken["status"] != null)
                    {
                        statusCode = (int)jToken["status"];
                    }
                    else
                    {
                        var statusProperty = responseObject.GetType().GetProperty("status");
                        if (statusProperty != null)
                        {
                            statusCode = (int)statusProperty.GetValue(responseObject);
                        }
                    }
                }
            }
            catch
            {
                statusCode = (int)HttpStatusCode.OK;
            }
            return this.Content((HttpStatusCode)statusCode, result);
        }
    }
}
