using Csp.Api.Services.Contracts;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;

namespace Csp.Api.Logic
{
    public class TrainingCreditsApiLogic : ITrainingCreditsApiLogic
    {
        private readonly IApplicationSettings _applicationSettings;

        public TrainingCreditsApiLogic(IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        public CommonApiResponse<dynamic> GetTrainingCredits(long userAccountId, long supportAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.TrainingCreditsEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetTrainingCourses(long userAccountId, long supportAccountId, string creditId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.TrainingCoursesEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId, creditId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetSelectedTrainingPartners(long userAccountId, long supportAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.SelectedTrainingPartnersEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        public CommonApiResponse<dynamic> GetTrainingPartners(long userAccountId, long supportAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.AvailableTrainingPartnersEndpoint, _applicationSettings.ReportingBffHost, supportAccountId, userAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        
        public CommonApiResponse<dynamic> UpdateSelectedTrainingPartners(TrainingPartner trainingPartner)
        {
            dynamic result = null;
            string baseUrl = string.Empty;
            try
            {
                var method = HttpMethod.Post.Method;
                var jsonPayload = JsonConvert.SerializeObject(trainingPartner);

                baseUrl = string.Format(ApiConstants.UpdateTrainingPartnerEndpoint, _applicationSettings.ReportingBffHost);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.ReportingBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
    }
}
