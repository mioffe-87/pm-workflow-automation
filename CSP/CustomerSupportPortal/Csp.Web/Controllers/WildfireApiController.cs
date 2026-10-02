using Csp.Api.Model.Enums;
using Csp.Api.Model.Wildfire;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    [Route("api/[controller]")]
    public class WildfireApiController : ApiController
    {
        static string ERROR_MESSAGE = "Error has occured during fetching data. Please try again later or contact System Administrator.";
        static string ERROR_MESSAGE_DATE = "Error has occured during fetching data. Start date is after end date.";
        private readonly IWildfireLogic wildfireLogic = null;
        private readonly ISecurityLogic _securityLogic;

        public WildfireApiController(IWildfireLogic wildfireLogic, ISecurityLogic securityLogic)
        {
            this.wildfireLogic = wildfireLogic;
            _securityLogic = securityLogic;
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public dynamic GetApiAssets(WildfireApiRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.SupportAccountId, request.UserAccountId, Permission.EditWildfireUsers))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to get Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                WildfireAssetsResponse<WildfireAssetItem> response = wildfireLogic.GetApiAssets(request);
                return Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of getting Wildfire Assets.");
            }
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public HttpResponseMessage CsvExport(WildfireApiRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.SupportAccountId, request.UserAccountId, Permission.EditWildfireUsers))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to export Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                string header = "Serial Number,IsTrial,ModelName,Quantity,Description,Expiration Date,Support License Type\r\n";
                string csv = string.Join("\r\n", wildfireLogic.GetApiAssets(request).Items.Select(i => i.Export()).ToArray());
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(header + csv)));
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "Wildfire_API_Assets_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/comma-separated-values");
                return response;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of exporting IONs.");
            }
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public dynamic GetPortals(WildfireApiRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting Wildfire Portals.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.SupportAccountId, request.UserAccountId, Permission.ViewSupportAccount))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to get Wildfire Portals.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                WildfireAssetsResponse<WildfirePortalItem> response = wildfireLogic.GetPortals(request);
                return Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of getting Wildfire Portals.");
            }
        }

        [JwtTokenAuthentication]
        [HttpPost] //, CspAuthorize(Permission.EditWildfireUsers)]
        public dynamic GetActivationDetails(WildfireApiActivationDetailsRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to get Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                return wildfireLogic.GetActivationDetails(request);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of getting Wildfire Assets.");
            }
        }

        [HttpGet]
        public string TestEncryptActivationDetails(string data)
        {
            if (data == null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = "Hash is required",
                    Success = "false"
                });
            }

            try
            {
                string hash = new Api.Logic.WildfireLogic(null, null).Encrypt(data);
                string encodedHash = System.Web.HttpUtility.UrlEncode(hash);

                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = encodedHash,
                    Success = "true"
                });
            }
            catch (Exception ex)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = ex.Message,
                    Success = "false"
                });
            }
        }

        [HttpGet]
        public string TestDecryptActivationDetails(string hash)
        {
            if (hash == null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = "Hash is required",
                    Success = "false"
                });
            }

            try
            {
                hash = System.Web.HttpUtility.UrlDecode(hash).Replace(" ", "+");
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = new Api.Logic.WildfireLogic(null, null).Decrypt(hash),
                    Success = "true"
                });
            }
            catch (Exception ex)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = ex.Message,
                    Success = "false"
                });
            }
        }

        [HttpGet]
        public string TestActivationDetails(string hash, long userAccountId)
        {
            if (hash == null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = "Hash is required",
                    Success = "false"
                });
            }

            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(wildfireLogic.GetActivationDetails(new WildfireApiActivationDetailsRequest
                {
                    UserAccountId = userAccountId, 
                    Hash = hash
                }));
            }
            catch (Exception ex)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Message = ex.Message,
                    Success = "false"
                });
            }
        }

        // [JwtTokenAuthentication]
        [HttpGet] //, CspAuthorize(Permission.EditWildfireUsers)]
        public WildfireActivationLicense GetActivationDetailsBasics(string authCode, int productType)
        {
            if (string.IsNullOrWhiteSpace(authCode))
            {
                var ex = new ArgumentException("Error of getting GetActivationDetailsBasics details.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            return wildfireLogic.GetProductTypeBasedDetails(new WildfireApiActivationDetailsRequestData
            {
                AuthCode = authCode,
                ProductType = (WildFireApiActivationProductType)productType
            });
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpPut]
        public dynamic Activate(WildfireApiActivateRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.CspId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.CspId, request.UserAccountId, Permission.EditWildfireUsers))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to get Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            return wildfireLogic.Activate(request);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public dynamic GetWildfireKeys(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditWildfireUsers))
            {
                return Unauthorized();
            }

            try
            {
                var wildfireApiKeys = wildfireLogic.GetWildfireKeys(supportAccountId);

                return Json(wildfireApiKeys);
            }
            catch
            {
                var httpResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error of getting Wildfire Api Keys.");
                return ResponseMessage(httpResponse);
            }
        }

        public WfApiConsumptionHealh CheckConsumptionHealth(long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                var ex = new UnauthorizedAccessException("WildfireController::CheckConsumptionHealth - User does not have permissions to check Wildfire Health.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            return wildfireLogic.CheckConsumptionHealth();
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public WfApiConsumptionSettings GetConsumptionSettings(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditWildfireUsers))
            {
                var ex = new UnauthorizedAccessException("WildfireController::GetConsumptionSettings.request - User does not have permissions to get Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (supportAccountId <= 0)
            {
                var ex = new ArgumentException("WildfireController::GetConsumptionSettings Support Account ID does not exist.", "supportAccountId");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            return wildfireLogic.GetConsumptionSettings(supportAccountId);
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public WfApiConsumptionResponse GetConsumptionData([FromBody]WfApiConsumptionRequest request)
        {
            if (request == null)
            {
                return new WfApiConsumptionResponse { Message = ERROR_MESSAGE };
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.SupportAccountId, request.UserAccountId, Permission.EditWildfireUsers))
            {
                var ex = new UnauthorizedAccessException("WildfireController::GetConsumptionData.request - User does not have permissions to get Wildfire Assets.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (request.SupportAccountId <= 0)
            {
                return new WfApiConsumptionResponse { SupportAccountId = request.SupportAccountId, Message = ERROR_MESSAGE };
            }

            if (request.StartDate.HasValue
                && request.EndDate.HasValue
                && request.StartDate.Value > request.EndDate.Value)
            {
                return new WfApiConsumptionResponse { SupportAccountId = request.SupportAccountId, Message = ERROR_MESSAGE_DATE };
            }

            if (!request.StartDate.HasValue)
            {
                request.StartDate = DateTime.Today.AddDays(-91);
            }

            if (!request.EndDate.HasValue)
            {
                request.EndDate = DateTime.Today.AddDays(-1);
            }


            return wildfireLogic.GetConsumptionData(request);
        }
    }
}
