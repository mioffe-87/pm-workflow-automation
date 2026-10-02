using System;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.HelperClasses;
using System.Net.Http;
using Newtonsoft.Json;
using System.Configuration;
using Newtonsoft.Json.Converters;

namespace Csp.Api.Logic
{
    public class LicensingApiLogic : ILicensingApiLogic
    {
        private readonly ILicensingApiRepository _licensingApiRepository;
        public LicensingApiLogic(ILicensingApiRepository iLicensingApiRepository)
        {
            _licensingApiRepository = iLicensingApiRepository;
        }

        private readonly string apiManagementURL = string.Format("{0}/{1}",ConfigurationManager.AppSettings["PortalBffHost"], "key-management/v1/");

        private readonly string apiManagementThumbPrint = ConfigurationManager.AppSettings["PortalBffCertThumbPrint"];

        private string GetBaseUrl(string urlPath, long userAccountId)
        {
            return string.Format("{0}{1}{2}{3}", apiManagementURL, urlPath, "&user_account_id=", userAccountId);
        }

        public ManageApiListDto GetAllApiDetails(long supportAccountId, long userAccountId)
        {
            var response = CommonHelper.ProcessWebRequest<ManageApiListDto>(GetBaseUrl(string.Format("{0}{1}","api-keys?support_account_id=", supportAccountId), userAccountId),
               HttpMethod.Get.Method, apiManagementThumbPrint, "");
            return response.ResponseObject;
        }

        public ModifyApiDto CreateLicensingApiKey(PostApiDetails postData)
        {

            var jsonPayload = JsonConvert.SerializeObject(postData);
            var response = CommonHelper.ProcessWebRequest<ModifyApiDto>(string.Format("{0}{1}", apiManagementURL, "api-key"),
                HttpMethod.Post.Method, apiManagementThumbPrint, jsonPayload);
            return response.ResponseObject;
        }

        public ModifyApiDto ExtendLicensingApiKey(PostDeleteApiDetails postData)
        {
            //The Datetime format is needed for java api to consume.
            var jsonPayload = JsonConvert.SerializeObject(postData, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
            var response = CommonHelper.ProcessWebRequest<ModifyApiDto>(string.Format("{0}{1}", apiManagementURL, "api-key"), HttpMethod.Post.Method, apiManagementThumbPrint, jsonPayload);
            return response.ResponseObject;
        }

        public ModifyApiDto DisableLicensingApiKey(PostDeleteApiDetails postData)
        {
            postData.ExpirationDate = null;
            var jsonPayload = JsonConvert.SerializeObject(postData);
            var response = CommonHelper.ProcessWebRequest<ModifyApiDto>(string.Format("{0}{1}",apiManagementURL,"api-key"),HttpMethod.Post.Method, apiManagementThumbPrint, jsonPayload);
            return response.ResponseObject;
        }

        public LicensingApi GetLicensingApiKey(long supportAccountId)
        {
            return _licensingApiRepository.GetLicensingApiKey(supportAccountId);
        }
    }
}
