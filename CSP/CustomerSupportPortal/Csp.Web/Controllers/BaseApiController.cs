using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Ion;
using Csp.Api.Model.LogsAndNotifications;
using Csp.Api.Model.Xsoar;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Newtonsoft.Json;
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
    public class BaseApiController : ApiController
    {
        private readonly IApplicationSettings applicationSettings = null;

        public BaseApiController(IApplicationSettings applicationSettings)
        {
            this.applicationSettings = applicationSettings;
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public void BffLogInfo(string source, string log, EBffLogLevel level = EBffLogLevel.INFO)
        {
            BffLogInfo(new BffLogInfo
            {
                Log = log,
                BffLogLevel = level,
                Source = source,
                Date = DateTime.Now
            });
        }

        [JwtTokenAuthentication]
        [HttpPost]
        public void BffLogInfo(BffLogInfo logInfo)
        {
            var jsonPayload = JsonConvert.SerializeObject(logInfo);
            CommonHelper.ProcessWebRequest<dynamic>(applicationSettings.EntitlementServiceBffHost + "/v1/log-info",
                HttpMethod.Post.Method, applicationSettings.EntitlementBffThumbprint, jsonPayload, true);
        }
    }
}
