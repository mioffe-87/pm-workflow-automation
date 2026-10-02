using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;

namespace Csp.Api.Logic
{
    public class ImpersonateRequestLogic:IImpersonateRequestLogic
    {
        public readonly string _enterpriseOktaBaseUrl = ConfigurationManager.AppSettings["EnterpriseOktaBaseUrl"];
        public readonly string _enterpriseOktaauthHeader = ConfigurationManager.AppSettings["EnterpriseOktaAuthKey"];
        public readonly string _impersonateAppId = ConfigurationManager.AppSettings["ImpersonateAppId"];
        private readonly IUserAccountLogic _userAccountLogic;

        public ImpersonateRequestLogic(IUserAccountLogic userAccountLogic)
        {
            _userAccountLogic = userAccountLogic;
        }

        public CommonApiResponse<dynamic> ProcessRequest(ImpersonateRequest request)
        {
            dynamic result = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.ImpersonateServiceUrl, ConfigurationManager.AppSettings["PortalBffHost"]); ;
                string certThumbPrint = ConfigurationManager.AppSettings["PortalBffCertThumbPrint"];
                string method = string.Empty;
                string jsonPayload = string.Empty;

                if (string.Equals(request.ImpersonateAction ,ImpersonateAction.Create.ToDescription(),StringComparison.InvariantCultureIgnoreCase))
                {
                    method = HttpMethod.Post.Method;
                    baseUrl = string.Format("{0}/{1}", baseUrl, "impersonate-request");
                    jsonPayload = JsonConvert.SerializeObject(request);
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.Update.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    method = "PATCH";
                    baseUrl = string.Format("{0}/{1}", baseUrl, "impersonate-request");
                    jsonPayload = JsonConvert.SerializeObject(request);
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.Get.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    long requestedByUserAccountId = GetRequestedByUserAccountId(request.ImpersonateRequestId, request.RequestorUserAccountId);
                    method = HttpMethod.Get.Method;
                    baseUrl = string.Format(ApiConstants.ImpersonateServiceV2Url, ConfigurationManager.AppSettings["PortalBffHost"]);
                    baseUrl = string.Format("{0}/{1}/{2}?user_account_id={3}", baseUrl, "impersonate-details", request.ImpersonateRequestId, requestedByUserAccountId);
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.History.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    method = HttpMethod.Get.Method;
                    baseUrl = string.Format("{0}/{1}/{2}{3}", baseUrl, "tac-page", request.RequestorUserAccountId, "?tab_name=requestsHistory");
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.ActiveRequests.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    method = HttpMethod.Get.Method;
                    baseUrl = string.Format("{0}/{1}/{2}{3}", baseUrl, "tac-page", request.RequestorUserAccountId, "?tab_name=activeRequests");
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.AddApprovers.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    method = HttpMethod.Post.Method;
                    baseUrl = string.Format("{0}/{1}", baseUrl, "impersonate-request/approvers");
                    jsonPayload = JsonConvert.SerializeObject(request);
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.ValidRequests.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    method = HttpMethod.Post.Method;
                    jsonPayload = JsonConvert.SerializeObject(request);
                    baseUrl = string.Format("{0}/{1}", baseUrl, "impersonate-request/account-switcher");
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.Extend.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    method = "PATCH";
                    jsonPayload = JsonConvert.SerializeObject(request);
                    baseUrl = string.Format("{0}/{1}", baseUrl, "impersonate-request/extend-expiration-date");
                }

                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, certThumbPrint, jsonPayload);
                
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    string jsonResponse = reader.ReadToEnd();

                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        return JsonConvert.DeserializeObject<CommonApiResponse<dynamic>>(jsonResponse, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public CommonApiResponse<dynamic> ProcessAutoApprovals(AutoApproveRequest request)
        {
            dynamic result = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.ImpersonateServiceUrl, ConfigurationManager.AppSettings["PortalBffHost"]); ;
                string certThumbPrint = ConfigurationManager.AppSettings["PortalBffCertThumbPrint"];
                string method = string.Empty;
                string jsonPayload = string.Empty;

                if (string.Equals(request.ImpersonateAction, ImpersonateAction.Approve.ToDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    method = HttpMethod.Post.Method;
                }
                else if (string.Equals(request.ImpersonateAction, ImpersonateAction.Revoke.ToDescription(),
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    method = "PATCH";

                }
                baseUrl = string.Format("{0}/{1}", baseUrl, "sfdc/impersonation");
                var filterModel = new AutoApproveFilteredRequest
                {
                    CaseOwnerEmailAddress = request.CaseOwnerEmailAddress,
                    RequestId = request.RequestId,
                    SalesforceCaseNumber = request.SalesforceCaseNumber,
                    Source = request.Source,
                    TargetCspSupportAccountId = request.TargetCspSupportAccountId,
                    TargetUserEmailAddress = request.TargetUserEmailAddress
                };
                jsonPayload = JsonConvert.SerializeObject(filterModel);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, certThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    string jsonResponse = reader.ReadToEnd();

                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        return JsonConvert.DeserializeObject<CommonApiResponse<dynamic>>(jsonResponse, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public bool HasValidImpersonateRequest(ImpersonateRequest request, out string requestId)
        {
            requestId = string.Empty;
            try
            {
                var response = ProcessRequest(request);
                if (response != null)
                {
                    if (response.ResponseObject != null)
                    {
                        //AccessibleAccounts accounts = new AccessibleAccounts();
                        //accounts = JsonConvert.DeserializeObject<AccessibleAccounts>(JsonConvert.SerializeObject(response.ResponseObject));

                        //if (accounts == null)
                        //    return false;

                        //return accounts.Accounts.Select(x => x == request.TargetSupportAccountId).Any();
                        ActiveRequestList result = JsonConvert.DeserializeObject<ActiveRequestList>(JsonConvert.SerializeObject(response.ResponseObject));
                        if(result != null)
                        {
                            ActiveRequests filteredList = result.activeRequests.Find(x => x.targetSupportAccountId == request.TargetSupportAccountId && 
                                                                                    x.requestedBy.ToLower() == request.RequestorEmailAddress.ToLower() && 
                                                                                    x.targetUserAccount.ToLower() == request.TargetEmailAddress.ToLower());
                            if (filteredList != null)
                            {
                                requestId = filteredList.requestId;
                                return filteredList.targetSupportAccountId > 0 && filteredList.expirationDate >= DateTime.Now;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }

        public bool UpdateDataForImpersonate(string sourceEmail, string targetUserEmail)
        {
            try
            {
                string userId = GetUserIdFromEnterpriseOkta(sourceEmail);
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }
                UpdateEmailOnEnterpriseOkta(userId, targetUserEmail, sourceEmail);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;

            }
            return true;
        }

        private string GetUserIdFromEnterpriseOkta(string userEmail)
        {
            dynamic result = null;
            SecurityDataAccess securityDataAccess = new SecurityDataAccess();
            long logId =0 ;
            try
            {
                string requestUrl = string.Format(ApiConstants.GetUserFromOkta, _enterpriseOktaBaseUrl, userEmail);
                //log api call
                logId = securityDataAccess.LogApiCall(new LogApiCall()
                {
                    ApiCalled = requestUrl,
                    ApiMethod = HttpMethod.Get.Method.ToString(),
                    Application = ApiConstants.CSP_APPLICATION,
                    CreatedBy = userEmail,
                    CreatedOn = DateTime.Now
                });
                
                result = CommonHelper.ProcessWebRequestWithAuthHeaders(requestUrl, HttpMethod.Get.Method, string.Empty,_enterpriseOktaauthHeader);

                //log response
                securityDataAccess.UpdateLogApi(new LogApiCall()
                {
                    ResponseStatus = HttpStatusCode.OK.ToString(),
                    ResponsePayload = result.ToString(),
                    LogApiCallId = logId
                });
            }
            catch (Exception ex)
            {
                //log error
                securityDataAccess.UpdateLogApi(new LogApiCall()
                {
                    ResponseStatus = "Exception",
                    ResponsePayload = ex.Message,
                    LogApiCallId = logId
                });

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
            if (result != null)
            {
                return result.id;
            }
            return result;
        }

        private void UpdateEmailOnEnterpriseOkta(string userId, string email, string sourceEmail)
        {
            SecurityDataAccess securityDataAccess = new SecurityDataAccess();
            long logId = 0;
            try
            {
                string requestUrl = string.Format(ApiConstants.UpdateUserOnOkta, _enterpriseOktaBaseUrl, _impersonateAppId, userId);
                string jsonPayload = JsonConvert.SerializeObject(new updateOktaUser()
                {
                    credentials = new credentials()
                    {
                        userName = email
                    }
                });

                logId = securityDataAccess.LogApiCall(new LogApiCall()
                {
                    ApiCalled = requestUrl,
                    ApiMethod = HttpMethod.Post.Method.ToString(),
                    Application = ApiConstants.CSP_APPLICATION,
                    RequestPayload = jsonPayload,
                    CreatedBy = sourceEmail,
                    CreatedOn = DateTime.Now
                });
                

                var response = CommonHelper.ProcessWebRequestWithAuthHeaders(requestUrl, HttpMethod.Post.Method, jsonPayload, _enterpriseOktaauthHeader);
                securityDataAccess.UpdateLogApi(new LogApiCall()
                {
                    ResponseStatus = HttpStatusCode.OK.ToString(),
                    ResponsePayload = Convert.ToString(response),
                    LogApiCallId = logId
                });
            }
            catch (Exception ex)
            {
                securityDataAccess.UpdateLogApi(new LogApiCall()
                {
                    ResponsePayload = ex.Message,
                    LogApiCallId = logId,
                    ResponseStatus = "Exception"
                });
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private dynamic GetImpersonateDetails(string impersonateRequestId)
        {
            string baseUrl = string.Format(ApiConstants.ImpersonateServiceUrl, ConfigurationManager.AppSettings["PortalBffHost"]);
            string certThumbPrint = ConfigurationManager.AppSettings["PortalBffCertThumbPrint"];
            string method = HttpMethod.Get.Method;
            baseUrl = string.Format("{0}/{1}/{2}", baseUrl, "impersonate-details", impersonateRequestId);
            dynamic result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, certThumbPrint, string.Empty);
            return result;
        }

        private long GetRequestedByUserAccountId(string impersonateRequestId, long defaultUserAccountId)
        {
            var impersonateDetails = GetImpersonateDetails(impersonateRequestId);
            if (impersonateDetails?.ResponseObject == null)
            {
                return defaultUserAccountId;
            }

            string requestedByEmail = impersonateDetails.ResponseObject.requestedBy != null ? (string)impersonateDetails.ResponseObject.requestedBy : string.Empty;
            string targetUserEmail = impersonateDetails.ResponseObject.targetUserAccount != null ? (string)impersonateDetails.ResponseObject.targetUserAccount : string.Empty;

            long requestedByUserAccountId = GetUserAccountIdByEmail(requestedByEmail);
            long targetUserAccountId = GetUserAccountIdByEmail(targetUserEmail);

            if (requestedByUserAccountId > 0 && (targetUserAccountId == defaultUserAccountId || requestedByUserAccountId == defaultUserAccountId))
            {
                return requestedByUserAccountId;
            }

            return defaultUserAccountId;
        }

        private long GetUserAccountIdByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return 0;
            }

            var userAccount = _userAccountLogic.GetUserAccountByEmail(email);
            return userAccount != null && userAccount.Id > 0 ? userAccount.Id : 0;
        }
    }
}
