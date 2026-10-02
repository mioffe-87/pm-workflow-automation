using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Web.DataTransferObjects;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;

namespace Csp.Web.Controllers
{
    public class LicenseApiController : ApiController
    {

        private readonly ILicensingApiLogic _iLicensingApiLogic;

        public LicenseApiController(ILicensingApiLogic iLicensingApiLogic)
        {
            _iLicensingApiLogic = iLicensingApiLogic;
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetApiKey(long supportAccountId, long userAccountId)
        {
            var securityDataAccess = new SecurityDataAccess();

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !securityDataAccess.IsSuperUser(supportAccountId, userAccountId))
            {
                return Unauthorized();
            }

            var returnData = new ManageApiListDto();
            try
            {
                returnData = _iLicensingApiLogic.GetAllApiDetails(supportAccountId, userAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }


        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult LicensingApi(PostDeleteApiDetails postData)
        {
            var securityDataAccess = new SecurityDataAccess();

            if (!JwtTokenManager.IsAuthorized(postData.UserAccountId, postData.SupportAccountId, ClaimType.UserAccountId) || !securityDataAccess.IsSuperUser(postData.SupportAccountId, postData.UserAccountId))
            {
                return Unauthorized();
            }

            long supportAccountId = postData.SupportAccountId;
            long userAccountId = postData.UserAccountId;
            string actionType = Convert.ToString(postData.ActionType).ToUpper();

            ModifyApiDto returnData;
            var result = new ModifyApiDto();
            string message = string.Empty;

            try
            {
                //Both Regenerate and Enable needs to call create.
                if (actionType == LicensingApiAction.ENABLE.ToString() || actionType == LicensingApiAction.REGENERATE.ToString())
                {
                    var postCreateData = new PostApiDetails()
                    {
                        ActionType = "create",
                        ProductId = postData.ProductId,
                        SupportAccountId = postData.SupportAccountId,
                        UserAccountId = postData.UserAccountId,
                    };

                    result = _iLicensingApiLogic.CreateLicensingApiKey(postCreateData);
                    message = actionType == LicensingApiAction.ENABLE.ToString() ? "Key is Created Successfully" : "Key is Regenerated Successfully";                    
                }
                else if (actionType == LicensingApiAction.EXTEND.ToString())
                {
                    if (postData.ExpirationDate.HasValue)
                    {
                        var maxExpirationDate = DateTime.Now.AddYears(2);
                        if (postData.ExpirationDate.Value > maxExpirationDate)
                        {
                            returnData = new ModifyApiDto()
                            {
                                IsSuccess = false,
                                ErrorMessage = "Expiration date cannot exceed 2 years from today."
                            };
                            return this.Json(returnData);
                        }
                    }
                    
                    message = "Key is Extended Successfully";
                    result = _iLicensingApiLogic.ExtendLicensingApiKey(postData);
                }
                else
                {
                    result = _iLicensingApiLogic.DisableLicensingApiKey(postData);
                    message = "Key is Disabled Successfully";
                }

                if (result != null)
                {
                    if(result.LicensingApiKeyId != null)
                    {
                        result.SuccessMessage = message;
                        result.IsSuccess = (string.IsNullOrEmpty(result.ErrorMessage) && string.IsNullOrEmpty(result.Reason));
                        returnData = result;
                    }
                    //Handle the case if the user is not superuser as Reason/ErrorMessage Exists.
                    else
                    {
                        result.IsSuccess = false;
                        returnData = result;
                    }
                }
                else
                {
                    returnData = new ModifyApiDto()
                    {
                        IsSuccess = false,
                        ErrorMessage = "An error occured. Please contact support"
                    };

                    string errorMsg = string.Format("Empty result for userAccountId : {0} , supportAccountId : {1} , actionType : {2}",
                            userAccountId, supportAccountId, actionType);

                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));
                }

            }
            catch (Exception ex) {

                returnData = new ModifyApiDto()
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occured. Please contact support"
                };

                string errorMsg = string.Format("Empty result for userAccountId : {0} , supportAccountId : {1} , actionType : {2} ",
                    userAccountId, supportAccountId, actionType);

                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }
    }
}