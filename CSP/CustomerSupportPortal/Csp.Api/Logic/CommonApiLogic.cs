using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Helpers;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace Csp.Api.Logic
{
    public class CommonApiLogic: ICommonApiLogic
    {
        private readonly IApplicationSettings _applicationSettings;

        public CommonApiLogic(IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }


        public async Task<CommonApiResponse<dynamic>> GetPendoDetails(PendoAccountDetails details)
        {
            dynamic result = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.PendoDetailsEndpoint, _applicationSettings.PortalBffHost);
                result = await CommonHelper.ProcessPendoWebRequestAsync<dynamic>(baseUrl, HttpMethod.Post.Method, _applicationSettings.PortalBffCertThumbPrint, JsonConvert.SerializeObject(details));
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<CommonApiResponse<dynamic>> GetSideNavigationMenuItems(long supportAccountId, long userAccountId)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.SideNavigationMenuEndpoint, _applicationSettings.PortalBffHost, userAccountId, supportAccountId);
                result = await CommonHelper.ProcessWebRequestAsync<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<CommonApiResponse<dynamic>> GetSideNavigationSubMenuItems(long supportAccountId, long userAccountId, int menuItemId)
        {
            dynamic result = null;

            try
            {
                string baseUrl = string.Format(ApiConstants.SideNavigationSubMenuEndpoint, _applicationSettings.PortalBffHost, userAccountId, supportAccountId, menuItemId);
                result = await CommonHelper.ProcessWebRequestAsync<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message);
            }
            return result;
        }

        public bool HasBetaAccess(long supportAccountId, int betaFeatureId)
        {
            bool hasAccess = false;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.BetaAccountsEndpoint, _applicationSettings.PortalBffHost);
                var result  = CommonHelper.ProcessWebRequest<BetaAccountsResponse>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty, true);

                foreach(var betaAccount in result.ResponseObject.BetaAccounts)
                {
                    hasAccess = betaAccount.BetaFeatureId == betaFeatureId && betaAccount.SupportAccountId == supportAccountId;
                    if (hasAccess)
                    {
                        break;
                    }
                }
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return hasAccess;
        }

        public CommonApiResponse<dynamic> ValidateAddress(AddressAPIRequest addressAPIRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(addressAPIRequest);
                var baseUrl = string.Format(ApiConstants.AddressValidationEndpoint, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> IsEligibleFor4HREsa(AddressAPIResponse address)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(address);
                var baseUrl = string.Format(ApiConstants.RMAEligibilityEndpoint, _applicationSettings.PortalBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetUserSelectedTheme(long userAccountId) {
            dynamic result = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.GetUserThemeEndpoint, _applicationSettings.PortalBffHost, userAccountId, "");
                result =  CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAiEligibility(long userAccountId, long supportAccountId)
        {
            dynamic result = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.GetAiEligibilityCheckEndpoint, _applicationSettings.PortalBffHost, userAccountId, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetAiAgentImpersonationStatus(long supportAccountId)
        {
            dynamic result = null;
            try
            {
                string baseUrl = string.Format(ApiConstants.GetAiAgentImpersonationStatusEndpoint, _applicationSettings.PortalBffHost, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        
        public async Task<CommonApiResponse<dynamic>> GetUnAuthNextGenMenuItems(long supportAccountId, long userAccountId)
        {
            try
            {
                string baseUrl = string.Format(ApiConstants.SideNavigationCombinedMenuEndpoint, _applicationSettings.PortalBffHost, userAccountId, supportAccountId);
                return await CommonHelper.ProcessWebRequestAsync<dynamic>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);
            }
            catch (WebException ex)
            {
                return ExceptionHelper.GetWebException(ex);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }
    }
}
  