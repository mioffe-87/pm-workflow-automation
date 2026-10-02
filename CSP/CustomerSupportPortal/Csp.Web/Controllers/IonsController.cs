using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Ion;
using Csp.Api.Model.Xsoar;
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
    public class IonsController : BaseApiController
    {
        private readonly IApplicationSettings applicationSettings = null;
        private readonly IIonLogic ionLogic = null;
        private readonly ISecurityLogic _securityLogic;

        public IonsController(IApplicationSettings applicationSettings, IIonLogic ionLogic, ISecurityLogic securityLogic)
            : base(applicationSettings)
        {
            this.ionLogic = ionLogic;
            _securityLogic = securityLogic;
        }

        [JwtTokenAuthentication]
        [HttpPost, CspAuthorize(Permission.ViewDevice)]
        public dynamic GetIons(IonRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting IONs.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.SupportAccountId, request.UserAccountId, Permission.ViewDevice))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to get IONs.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                IonResponse response = ionLogic.GetIons(request);
                if (request.DeviceType == EDeviceType.Physical
                    || request.DeviceType == EDeviceType.Virtual)
                {
                    response.Items.Where(p => p.IsParent).ToList().ForEach(i => i.Licenses = response.Items.Where(p => p.SerialNumber == i.SerialNumber && !p.IsParent).ToList());
                    response.Items = response.Items.Where(p => p.IsParent);
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of getting IONs.");
            }
        }

        [JwtTokenAuthentication]
        [HttpPost, CspAuthorize(Permission.ViewDevice)]
        public HttpResponseMessage CsvExport(IonRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting IONs.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.SupportAccountId, request.UserAccountId, Permission.ViewDevice))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to export IONs.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                string header = request.DeviceType == EDeviceType.Physical
                    ? "Tenant ID,Serial Number,AuthCode,Model Name,Description,Expiration Date\r\n"
                    : request.DeviceType == EDeviceType.Virtual
                        ? "Tenant ID,Serial Number,AuthCode,Model Name,Description,Expiration Date,Quantity\r\n"
                        : "Tenant ID,AuthCode,Model Name,Description,Expiration Date,Quantity\r\n";
                string csv = string.Join("\r\n", ionLogic.GetIons(request, true).Items.Select(i => i.Export(request.DeviceType)).ToArray());
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(header + csv)));
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = (request.DeviceType == EDeviceType.Physical ? "Physical" : request.DeviceType == EDeviceType.Virtual ? "Virtual" : "Add-On") + "_IONs_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/comma-separated-values");
                return response;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of exporting IONs.");
            }
        }

        [JwtTokenAuthentication]
        [HttpPost, CspAuthorize(Permission.ViewDevice)]
        public dynamic GetIonsPageData(IonsPageRequest request)
        {
            if (request == null)
            {
                var ex = new ArgumentException("Error of getting ION Page setup.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            if (!JwtTokenManager.IsAuthorized(request.UserAccountId, request.SupportAccountId, ClaimType.UserAccountId) || !_securityLogic.HasPermission(request.SupportAccountId, request.UserAccountId, Permission.ViewDevice))
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to get ION Page setup.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            try
            {
                return Json(ionLogic.GetIonsPageData(request));
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error of getting ION Page setup.");
            }
        }
    }
}
