using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Model.ConsumerProduct;
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
    public class ConsumerProductController : ApiController
    {
        private readonly IConsumerProductLogic _consumerProductLogic;
        public ConsumerProductController(IConsumerProductLogic consumerProductLogic)
        {
            this._consumerProductLogic = consumerProductLogic;
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public dynamic GetOkyoGarde([FromBody]ConsumerProductRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting Consumer Products OKYO Garde.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to get Consumer Products OKYO Garde.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                ConsumerProductResponse<ConsumerProductOkyoGardeParent> returnData = _consumerProductLogic.GetOkyoGarde(request);
                return Json(returnData);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of getting Consumer Products OKYO Garde.");
            }
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public HttpResponseMessage CsvExportOkyoGarde([FromBody]ConsumerProductRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting Consumer Products OKYO Garde.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to export Consumer Products OKYO Garde.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                string header = "Tenant Serial Number,Part Number,Description,Total Quantity,Total Activated Quantity,Total Available Quantity,Region,Service Level,Expiration Date,AuthCode,PartNumber,Description,Quantity,ActivatedQuantity,AvailableQuantity\r\n";
                string csv = string.Join("\r\n", _consumerProductLogic.GetOkyoGarde(request).Items.Select(i => i.Export()).ToArray());
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(header + csv)));
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "ConsumerProducts_OKYO_Garde_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/comma-separated-values");
                return response;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of exporting Consumer Products OKYO Garde.");
            }
        }
    }
}
