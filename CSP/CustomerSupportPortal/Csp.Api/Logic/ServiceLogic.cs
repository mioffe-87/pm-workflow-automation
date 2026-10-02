using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Net.Http;
using AutoMapper;

namespace Csp.Api.Logic
{
    public class ServiceLogic : BaseLogic, IServiceLogic

    {
        #region Private Members
        
        private readonly IApplicationSettings _applicationSettings;
        #endregion
        public ServiceLogic(IApplicationSettings applicationSetting)
        {
          
            _applicationSettings = applicationSetting;
        }
        #region Public Members
        public CommonApiResponse<dynamic> GetAlertsData()
        {
            dynamic result = null;
            try
            {
                var scope = string.Format(@"{0},{1}", (int)Scope.Authenticated, (int)Scope.Both);
                var jsonPayload = JsonConvert.SerializeObject(new { scopes = new List<int>(scope.Split(',').Select(Int32.Parse).ToList()) });
                string baseUrl = string.Format(ApiConstants.BannerMessagesEndpoint, _applicationSettings.PortalBffHost);                
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, HttpMethod.Post.Method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);                
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
        #endregion
    }
}
