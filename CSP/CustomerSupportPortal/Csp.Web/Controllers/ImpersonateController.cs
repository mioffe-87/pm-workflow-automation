using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Mvc;
using Csp.Api.DataAccess;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Newtonsoft.Json;

namespace Csp.Web.Controllers
{
    [System.Web.Mvc.Authorize]
    public class ImpersonateController : ApiController
    {
        private readonly IImpersonateRequestLogic _impersonateRequestLogic;

        public ImpersonateController(IImpersonateRequestLogic impersonateRequestLogic)
        {
            _impersonateRequestLogic = impersonateRequestLogic;
        }

        [System.Web.Http.HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult ProcessRequest([FromBody] ImpersonateRequest impersonateRequest)
        {
            if (!JwtTokenManager.IsAuthorized(impersonateRequest.RequestorUserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var result = _impersonateRequestLogic.ProcessRequest(impersonateRequest);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(string.Empty);
        }
        [System.Web.Http.HttpPost]
        public IHttpActionResult ProcessRequestV2([FromBody]AutoApproveRequest model)
        {
            try
            {
                var logDir = @"c:\logs";
                var logFile = @"C:\logs\impersonate_request_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ProcessRequestV2 Request:\n" +
                                 $"Email: {model.Email ?? "null"}\n" +
                                 $"TargetUserEmailAddress: {model.TargetUserEmailAddress ?? "null"}\n" +
                                 $"TargetCspSupportAccountId: {model.TargetCspSupportAccountId ?? "null"}\n" +
                                 $"CaseOwnerEmailAddress: {model.CaseOwnerEmailAddress ?? "null"}\n\n";

                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                File.AppendAllText(logFile, logMessage);

                var privateKey = ConfigurationManager.AppSettings["EncryptionProviderPrivateKey"];
                var encryptionProvider = new EncryptionProvider();

                var isKeyValid = encryptionProvider.IsKeyValid(model.Email, model.Key, privateKey);
                if (!isKeyValid)
                {
                    //Elmah.ErrorSignal.Raise(new Exception(String.Format("Invalid key {0}", model.Key)));
                    return Unauthorized();
                }

                var result = _impersonateRequestLogic.ProcessAutoApprovals(model);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                var errorMsg = "Error in call to ImpersonateAutoApproveReject() " + JsonConvert.SerializeObject(model);
                var exceptionDetails = ex.StackTrace?.ToString() ?? string.Empty;
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {errorMsg}\nException: {exceptionDetails}\n{ex.Message}\n{ex.StackTrace}\n\n";
              
                    var logDir = @"c:\logs";
                    var logFile = @"C:\logs\impersonate_" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";

                if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    File.AppendAllText(logFile, logMessage);
                
                
                return this.Json(string.Empty);
            }
        }
    }
}