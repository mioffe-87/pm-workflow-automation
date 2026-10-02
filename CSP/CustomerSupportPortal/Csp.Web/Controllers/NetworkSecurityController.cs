using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    public class NetworkSecurityController : ApiController
    {
        private readonly INetworkSecurityApiLogic _networkSecurityApiLogic;
        private readonly string data_collection_topic = ConfigurationManager.AppSettings.Get("Data_Collection_TopicName");
        private readonly string data_collection_project = ConfigurationManager.AppSettings.Get("Data_Collection_ProjectName");

        public NetworkSecurityController(INetworkSecurityApiLogic networkSecurityApiLogic)
        {
            _networkSecurityApiLogic = networkSecurityApiLogic;
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetFilters(long userAccountId, string filter, long supportAccountId, int? categoryId = null)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            if (!ValidateFilter(filter))
            {
                string message = string.Format(Constants.InvalidFilter, filter);
                return this.Json(new { data = new { success = false, message = message } });
            }

            try
            {
                var result = _networkSecurityApiLogic.GetFilters(filter, userAccountId, supportAccountId, categoryId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetDeviceTag(long supportAccountId, long userAccountId, string serialNumber)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetDeviceTag(supportAccountId, userAccountId, serialNumber);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetDeviceTagBySupportAccount(long userAccountId, [FromBody]DeviceTagRequest deviceTagRequest, int? page = null, int? size = null, string sort = "")
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, deviceTagRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetDeviceTag(page, size, sort, deviceTagRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAssetData(long userAccountId, [FromBody]AssetRequest asssetRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, asssetRequest.SupportAccountId, ClaimType.UserAccountId) ||  !(userAccountId == asssetRequest.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAssetData(asssetRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAssetMetaData(long userAccountId, [FromBody]AssetRequest asssetRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, asssetRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAssetMetaData(asssetRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAssetDashBoardData(long userAccountId, [FromBody]AssetRequest asssetRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, asssetRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAssetDashboardData(asssetRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAssetDetails(long userAccountId, [FromBody]AssetDetailsRequest assetDetailsRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, assetDetailsRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAssetDetails(assetDetailsRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetMasterAuthcodeDetails(long supportAccountId, long userAccountId, string authCode)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAuthCodeDetails(supportAccountId, userAccountId, authCode);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetNgfwFlexDetails(long userAccountId, [FromBody]FlexDetailsRequest flexDetailsRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, flexDetailsRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetFlexCreditDetails(flexDetailsRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetPanOSVersions(long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetPanOSVersions();
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult DownloadAssetData(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var downloadData = _networkSecurityApiLogic.DownloadAssetData(supportAccountId, userAccountId);
                return this.Json(downloadData);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPut]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateDeviceInformation(long userAccountId, [FromBody]DeviceInformation deviceInfo)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, deviceInfo.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.UpdateDeviceInformation(deviceInfo);
                return this.Json(result);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAscContactInformation(long userAccountId, long supportAccountId, long ascSupportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAscContactInformation(userAccountId, supportAccountId, ascSupportAccountId);
                return this.Json(result);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult AssociateComponent(long userAccountId, [FromBody]AssociateComponentRequest associateComponentRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, associateComponentRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.AssociateComponent(associateComponentRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GetUnRegisteredComponents(long userAccountId, [FromBody]ComponentRequest componentRequest)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, componentRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetUnRegisteredComponents(componentRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private HttpResponseMessage GetStringWebException(WebException ex)
        {
            string response = string.Empty;

            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
            {
                string jsonResponse = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    response = jsonResponse.ToString();
                }
            }
            dynamic result = JsonConvert.DeserializeObject(response);
            if (result != null)
            {
                return Request.CreateErrorResponse(((HttpWebResponse)ex.Response).StatusCode, ex);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, Constants.UnexpectedError);
            }
        }


        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult InitiateDataLoad(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                EventPublisher eventPublisher = new EventPublisher();
                eventPublisher.PublishCustomEvent(new
                {
                    support_account_id = supportAccountId,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    source = "csp"
                }, data_collection_project, data_collection_topic);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        #region Clai Sec Proxies
        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAssetAttributes(long userAccountId, string serialNumber)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GetAssetAttributes(serialNumber);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }
        #endregion


        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult GenerateMagicLink([FromBody]MagicLinkRequest magicLinkRequest)
        {

            if (!JwtTokenManager.IsAuthorized(magicLinkRequest.UserAccountId, magicLinkRequest.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.GenerateMagicLink(magicLinkRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult ChassisComponents(long userAccountId, long supportAccountId, string serialNumber)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = _networkSecurityApiLogic.ChassisComponents(userAccountId, supportAccountId, serialNumber);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        private static readonly string[] ForbiddenFilters = new[]
        {
            FilterDescription.SupportAccountId.ToDescription(),
            FilterDescription.UserAccountId.ToDescription(),
            FilterDescription.CategoryId.ToDescription(),
            FilterDescription.InternalRequest.ToDescription()
        };

        private bool ValidateFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            return !ForbiddenFilters.Any(forbidden => 
                filter.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) >= 0);
        }

    }

    public enum FilterDescription
    {
        [Description("support_account_id")]
        SupportAccountId,
        [Description("user_account_id")]
        UserAccountId,
        [Description("category_id")]
        CategoryId,
        [Description("internal_request")]
        InternalRequest,
    }

}
