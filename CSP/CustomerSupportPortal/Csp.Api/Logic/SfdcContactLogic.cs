using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Csp.Api.Logic
{
    public class SfdcContactLogic : ISfdcContactLogic
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly ISupportAccountUserLogic _supportAccountUserLogic;
        private readonly ICommonApiLogic _commonApiLogic;

        public SfdcContactLogic(IApplicationSettings applicationSettings, ISupportAccountUserLogic supportAccountUserLogic, ICommonApiLogic commonApiLogic)
        {
            _applicationSettings = applicationSettings;
            _supportAccountUserLogic = supportAccountUserLogic;
            _commonApiLogic = commonApiLogic;
        }

        public CommonApiResponse<SupportCaseNavigationResponse> ManageSfdcCaseCreationHistory(SupportCaseNavigationRequest request)
        {
            string userName = string.Empty;

            try
            {
                #region Auto Case Creation
                // if (!ValidateSfdcContact(request))
                // {
                //     var method = HttpMethod.Get.Method;
                //     var baseUrl = string.Format(ApiConstants.CaseCreationHistoryDetailsEndpoint, _applicationSettings.PortalBffHost, request.UserAccountId, request.SupportAccountId);
                //     var response = CommonHelper.ProcessWebRequest<LogSfdcCaseCreationHistoryResponse>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);

                //     if (!string.IsNullOrEmpty(response?.ResponseObject?.CaseNumber))
                //     {
                //         return GetCommonResponse(string.Empty, string.Format(ApiConstants.SupportCasePopupAlertMessage, response.ResponseObject.CaseNumber));
                //     }
                //     else
                //     {
                //SfdcCaseCreationRequest sfdcCaseCreationRequest = BuildSfdcCaseCreationRequest(request);

                //if (!string.IsNullOrEmpty(sfdcCaseCreationRequest?.Email))
                //{
                //    userName = sfdcCaseCreationRequest.FirstName + " " + sfdcCaseCreationRequest.LastName;
                //    SfdcCaseCreationResponse sfdcCaseCreationResponse = CreateCase(sfdcCaseCreationRequest);

                //    if (!string.IsNullOrEmpty(sfdcCaseCreationResponse?.CaseNumber))
                //    {
                //        LogSfdcCaseCreationHistoryRequest logSfdcCaseCreationHistoryRequest = new LogSfdcCaseCreationHistoryRequest()
                //        {
                //            CaseNumber = sfdcCaseCreationResponse.CaseNumber,
                //            UserAccountId = request.UserAccountId,
                //            SupportAccountId = request.SupportAccountId
                //        };

                //        LogCaseCreationHistory(logSfdcCaseCreationHistoryRequest);
                //        return GetCommonResponse(string.Empty, string.Format(ApiConstants.SupportCasePopupAlertMessage, sfdcCaseCreationResponse.CaseNumber));
                //    }
                //}
                //   }
                // }
                // else
                // {
                //     string link = request.IsSidenav ? _applicationSettings.SupportCasesUrl : _applicationSettings.CaseCreationUrl;
                //     return GetCommonResponse(link, string.Empty);
                // }
                #endregion

                string link = _applicationSettings.SupportCasesUrl; //set case creation as default link
                string message = string.Empty;
                if (!request.IsSidenav)
                {
                    if (string.IsNullOrEmpty(request.path))
                    {
                        link = _applicationSettings.CaseCreationUrl;
                    }
                    else
                    {
                        string path = request.path;
                        if (string.Equals(path, "strata", StringComparison.OrdinalIgnoreCase))
                        {
                            bool IsCopilotUser = false;
                            var aiEligible = _commonApiLogic.GetAiEligibility(request.UserAccountId, request.SupportAccountId);
                            if (aiEligible != null && aiEligible.ResponseObject != null)
                            {
                                var responseObjectAi = aiEligible.ResponseObject;
                                IsCopilotUser = Convert.ToBoolean(responseObjectAi.IsAIEligible);
                            }
                            if (IsCopilotUser)
                            {
                                link = string.Format(ApiConstants.CopilotUrl, _applicationSettings.ApplicationUrl, request.SupportAccountId);
                                message = link; //used to load the link in same window if message is same as link
                            }
                        }
                        else if (string.Equals(path, "admin", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(path, "cortex", StringComparison.OrdinalIgnoreCase))
                        {
                            link = string.Format(ApiConstants.GetHelpPathUrl, _applicationSettings.CaseCreationUrl, path);
                        }
                    }
                }
                return GetCommonResponse(link, message);
             
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            var loginHelp = string.Format(ApiConstants.LoginHelp, _applicationSettings.ApplicationUrl);
            return GetCommonResponse(loginHelp, string.Empty);
        }

        private bool ValidateSfdcContact(SupportCaseNavigationRequest navigationRequest)
        {
            try
            {
                if (navigationRequest.Email.Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
                {
                    return true;
                }

                dynamic request = new
                {
                    supportAccountId = navigationRequest.SupportAccountId,
                    userAccountId = navigationRequest.UserAccountId,
                    isCreateContact = false
                };
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.VerifySfdcContactEndpoint, _applicationSettings.PortalBffHost);
                var result = CommonHelper.ProcessWebRequest<MembershipSfdcContactValidationResponse>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
                if (result?.ResponseObject?.IsContactExists || !result?.ResponseObject?.CanCreateCase)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("ValidateSfdcContact " + ex.Message.ToString()));
            }
            return false;
        }

        private SfdcCaseCreationResponse CreateCase(SfdcCaseCreationRequest request)
        {
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.ContactSupportEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                var result  = CommonHelper.GetWebResponse<CommonApiResponse<SfdcCaseCreationResponse>>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
                return result.ResponseObject;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("CreateCase " + ex.Message.ToString()));
            }
            return new SfdcCaseCreationResponse();
        }

        private CommonApiResponse<SupportCaseNavigationResponse> GetCommonResponse(string link, string message)
        {
            return new CommonApiResponse<SupportCaseNavigationResponse>()
            {
                ResponseObject = new SupportCaseNavigationResponse()
                {
                  Link = link,
                  Message = message
                }
            };
        }

        private SfdcCaseCreationRequest BuildSfdcCaseCreationRequest(SupportCaseNavigationRequest request)
        {
           
            UserProfileModel user = _supportAccountUserLogic.GetUserData(request.Email);

            if(user?.UserAccountId > 0 && request.UserAccountId == user.UserAccountId)
            {
                return new SfdcCaseCreationRequest()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CompanyName = request.CompanyName,
                    PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber) ? "000" : user.PhoneNumber,
                    IssueType = ApiConstants.SupportCaseIssueType,
                    Description = string.Format(ApiConstants.SupportCaseIssueDescription, request?.SupportAccountId > 0 ? request.SupportAccountId.ToString() + "-"
                    + request?.CompanyName : string.Empty),
                    ErrorMessage = ApiConstants.SupportCaseErrorMessage
                };
            }

            return new SfdcCaseCreationRequest();
        }

        private void LogCaseCreationHistory(LogSfdcCaseCreationHistoryRequest request)
        {
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(request);
                var baseUrl = string.Format(ApiConstants.CaseCreationHistoryEndpoint, _applicationSettings.PortalBffHost);
                CommonHelper.ProcessRequest(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("LogCaseCreationHistory " + ex.Message.ToString()));
            }
        }
    }
}
