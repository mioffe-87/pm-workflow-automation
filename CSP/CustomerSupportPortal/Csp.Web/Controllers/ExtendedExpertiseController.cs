using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.ExtendedExpertise;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
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
    public class ExtendedExpertiseController : ApiController
    {
        private readonly IExtendedExpertiseLogic extendedExpertiseLogic;

        public ExtendedExpertiseController(IExtendedExpertiseLogic extendedExpertiseLogic)
        {
            this.extendedExpertiseLogic = extendedExpertiseLogic;
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetCoreData(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new ExtendedExpertiseCoreData();

            try
            {
                returnData = extendedExpertiseLogic.GetCoreData(supportAccountId, userAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetEngagements(long supportAccountId, long userAccountId, string status)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<ExtendedExpertiseItem>();

            try
            {
                returnData = extendedExpertiseLogic.GetEngagements(supportAccountId, userAccountId, status).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpGet, HttpOptions]
        public IHttpActionResult ExportEngagements(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            StringBuilder sb = new StringBuilder(ExtendedExpertiseItem.CsvHeader);

            extendedExpertiseLogic.GetEngagements(supportAccountId, userAccountId).ToList().ForEach(i => sb.AppendFormat("\r\n{0}", i.CsvRow));

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            writer.Write(sb.ToString());
            writer.Flush();
            stream.Position = 0;
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = string.Format("ExtendedExpertiseEngagements_{0}.csv", DateTime.Now.Date.ToString("yyyyMMdd"))
            };

            return ResponseMessage(response);
        }
    }
}
