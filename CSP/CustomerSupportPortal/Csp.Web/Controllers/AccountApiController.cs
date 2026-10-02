using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Api.Security;
using Csp.Web.Security.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    public class AccountApiController : ApiController
    {
        private readonly ISupportAccountUserLogic _supportAccountUserLogic;
        private readonly IHomeLogic _homeLogic;
        private readonly ISecurityLogic _securityLogic;

        public AccountApiController(ISupportAccountUserLogic supportAccountUserLogic, IHomeLogic homeLogic, ISecurityLogic securityLogic)
        {
            _supportAccountUserLogic = supportAccountUserLogic;
            _homeLogic = homeLogic;
            _securityLogic = securityLogic;
        }

        #region Account Details Proxies


        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetSupportAccountDetails(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetSupportAccountDetails(supportAccountId, true);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPatch]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateSupportAccountInfo(long userAccountId, long supportAccountId, [FromBody]SupportAccountInfo supportAccountInfo)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.UpdateSupportAccountInfo(supportAccountId, userAccountId, supportAccountInfo);
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
        public IHttpActionResult GetDefaultRoles(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetDefaultRoles(supportAccountId, userAccountId, true);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPatch]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateDefaultRoles(long userAccountId, long supportAccountId, [FromBody]RolesInfo roles)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.UpdateDefaultRoles(supportAccountId, userAccountId, roles);
                var statusCode = GetStatusCodeFromResponse(result);
                return this.Content((HttpStatusCode)statusCode, result);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetSelfRegistrationDetails(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetSelfRegistrationDetails(supportAccountId, userAccountId);
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
        public IHttpActionResult GetAscInformation(long userAccountId, long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.GetAscInformation(supportAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }
        [HttpPut]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateAscInformation(long userAccountId, long supportAccountId, [FromBody]UpdateAscInformation ascInfo)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.UpdateAscInformation(supportAccountId, ascInfo);
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
        public IHttpActionResult CreateSelfRegistrationDetails(long userAccountId, [FromBody]SelfRegistration info)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, info.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(info.SupportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.CreateSelfRegistrationDetails(info);
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
        public IHttpActionResult ConvertEvalAccount(long userAccountId, long supportAccountId, [FromBody]EvalAccountDetails info)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                info.SupportAccountId = supportAccountId;
                info.UserAccountId = userAccountId;
                var result = _supportAccountUserLogic.ConvertEvalAccount(info);
                return this.Json(result);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPatch]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateSelfRegistrationDetails(long userAccountId, [FromBody]UpdateSelfRegistration info)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, info.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(info.SupportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.UpdateSelfRegistrationDetails(info);
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
        public IHttpActionResult GetSSOInformation(long userAccountId, long supportAccountId, string email)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
               
                var result = _supportAccountUserLogic.GetSSOInformation(userAccountId, supportAccountId, email);
                if (result!=null && result.ShowUrl)
                {
                    var thirdPartyIdpConfiguration = SiteHelper.GetThirdPartyIdpConfiguration(result.domain);
                    var ThirdPartyIdpEntityId = (thirdPartyIdpConfiguration != null) ? thirdPartyIdpConfiguration.entityId : null;
                    if (ThirdPartyIdpEntityId != null)
                    {
                        result.IsConfigured = true;
                    }
                  
                }
                return this.Json(result);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult ChangeAccount(long supportAccountId, long userAccountId, int page, int pageSize,string sort = "", string searchField = "", string searchValue = "")
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.ViewSupportAccount))
            {
                return Unauthorized();
            }

            try
            {

                var dataParameters = new DataParameters();
                int pageSizeVal = 10;
                if (pageSize == 0)                
                    pageSize = pageSizeVal;                       
              
                dataParameters.PaginationCount = pageSize;
                dataParameters.PaginationOffset = (page - 1) * pageSize;
                // default to CompanyName               
                if (!string.IsNullOrEmpty(sort))
                {
                    string[] splitString = sort.Split('-');
                    dataParameters.SortField = splitString[0];                    
                    dataParameters.SortDirection = splitString[1] == "asc"?SortDirection.Ascending : SortDirection.Descending;
                }                             
                dataParameters.SearchField = searchField;
                dataParameters.SearchValue = searchValue;
                var accounts = _homeLogic.GetAvailableAccounts(userAccountId, dataParameters, setRoles: true);
                var result = new MyAccountsResponse
                {
                    Data = accounts,
                    total = Convert.ToInt32(dataParameters.TotalRecordCount)

                };
                return this.Json(result);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);

        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult ChangeAccountReport(long userAccountId, string searchField = "", string searchValue = "")
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

                var csv = _homeLogic.GetAvailableAccountsCsv(userAccountId, searchField, searchValue);
                var str = System.Text.Encoding.Default.GetString(csv);
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(str.ToString());
                writer.Flush();
                stream.Position = 0;
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition,X-Suggested-Filename");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = string.Format("AccountsReport_{0}.csv", DateTime.Now.Date.ToString("yyyyMMdd"))
                };

                return ResponseMessage(response);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                var httpResponse = Request.CreateErrorResponse(HttpStatusCode.NotFound, Constants.UnexpectedError);
                return ResponseMessage(httpResponse);
            }
        }

        [HttpPatch]
        [JwtTokenAuthentication]
        public IHttpActionResult UpdateEmailDomainType(long userAccountId, long supportAccountId, [FromBody]UpdateEmailDomain emailDomainInfo)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(supportAccountId, userAccountId, Permission.EditSupportAccount))
            {
                return Unauthorized();
            }
            try
            {
                var result = _supportAccountUserLogic.UpdateEmailDomainType(supportAccountId, emailDomainInfo);
                return this.Json(result);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        #endregion

        private int GetStatusCodeFromResponse(CommonApiResponse<dynamic> result)
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
            return statusCode;
        }

    }
}