using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    [Route("api/[controller]")]
    public class ZtpController : ApiController
    {
        private readonly IZtpLogic _ztpLogic;
        private readonly string _panoramaValidationMsg = "Selected panoramas cannot be the same, please correct your selection.";
        public ZtpController(IZtpLogic ztpLogic)
        {
            _ztpLogic = ztpLogic;
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetPanoramaAssociations(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<PanoramaAssociationResult>();

            try
            {
                returnData = _ztpLogic.GetPanoramaAssociations(supportAccountId).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult AddOrUpdatePanoramaAssociation(PanoramaAssociationRequest requestData)
        {
            var status = false;
            string message = string.Empty;
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId, requestData.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                var isPrimaryValid=  _ztpLogic.IsPanormaDeviceActive(requestData.SupportAccountId, requestData.PrimarySerialNumber);
                var isSecondaryValid = string.IsNullOrEmpty(requestData.SecondarySerialNumber) ? true :
                                     _ztpLogic.IsPanormaDeviceActive(requestData.SupportAccountId, requestData.SecondarySerialNumber);
                
                if(!isPrimaryValid && !isSecondaryValid)
                {
                    message = "Primary & Secondary serial numbers are in-active. Please select active Serial numbers.";
                }
                else if (!isPrimaryValid)
                {
                    message = "Primary serial number is in-active. Please select active Serial number.";
                }
                else if (!isSecondaryValid)
                {
                    message = "Secondary serial number is in-active. Please select active Serial number.";
                }
                else if (requestData.PrimarySerialNumber == requestData.SecondarySerialNumber)
                {
                    message = _panoramaValidationMsg;
                }
                else
                {
                    status = _ztpLogic.AddOrUpdatePanoramaAssociation(requestData);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(new { Success = status, Message = message });
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetPanoramaDeviceSerialNumbers(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            { 
            
                return Unauthorized();
            }

            var serialNumbers = new List<NameValueOnly>();

            try
            {
                serialNumbers = _ztpLogic.GetDevicesForZtp(supportAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(serialNumbers);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public IHttpActionResult GetPanoramaAssociationHistory(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var associationHistory = new List<PanoramaAssociationHistory>();

            try
            {
                associationHistory = _ztpLogic.GetPanoramaAssociationHistory(supportAccountId).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(associationHistory);
        }
    }
}
