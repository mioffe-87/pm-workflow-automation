using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Api.Model.PartialShipment;
using Csp.Api.HelperClasses;
using Csp.Api.Security;
using Csp.Web.Security.Attributes;
using Csp.Web.HelperClasses;
using Csp.Api.Model.Enums;

namespace Csp.Web.Controllers
{
    public class PartialShipmentController : ApiController
    {
        private readonly IPartialShipmentLogic _partialShipmentLogic;
        private readonly ISecurityLogic _securityLogic;

        public PartialShipmentController(IPartialShipmentLogic partialShipmentLogic, ISecurityLogic securityLogic)
        {
            _partialShipmentLogic = partialShipmentLogic;
            _securityLogic = securityLogic;
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult MasterContractList(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response = _partialShipmentLogic.GetMasterContractList(supportAccountId, userAccountId);
            return Json(response);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult MasterContractDetails(string masterOrderNumber, long userAccountId,
            long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response = _partialShipmentLogic.GetMasterContractDetails(masterOrderNumber, userAccountId);
            return Json(response);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult OrderContractDetails(string masterOrderNumber, long userAccountId,
            long supportAccountId, bool isDataCenter)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response = _partialShipmentLogic.GetOrderContractDetails(masterOrderNumber, userAccountId,isDataCenter);
            return Json(response);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult ShipmentHistory(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response = _partialShipmentLogic.GetShipmentHistory(supportAccountId);
            return Json(response);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult ShippingOrderDetail(string sapOrderNumber, long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response =
                _partialShipmentLogic.GetShippingOrderDetail(sapOrderNumber, supportAccountId, userAccountId);
            return Json(response);
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult CreatePartialShipmentOrder([FromBody] PartialShipmentOrder requestData,
            long supportAccountId, long userAccountId)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            PartialShipmentOrderResult partialShipmentOrderResult;
            try
            {
                partialShipmentOrderResult =
                    _partialShipmentLogic.CreatePartialShipmentOrder(requestData, supportAccountId, userAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

                partialShipmentOrderResult = new PartialShipmentOrderResult
                {
                    IsSuccess = false,

                    //ToDo: Remove hardcoded error message.
                    ErrorMessage = "Cannot save partialShipmentOrder"
                };
            }

            return Json(partialShipmentOrderResult);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult Addresses(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response = _partialShipmentLogic.GetAddresses(supportAccountId);
            return Json(response);
        }


        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult ShippingMethodList(bool IsDomestic)
        {
           
            var response = _partialShipmentLogic.GetShippingMethodList(IsDomestic);
            return Json(response);
        }


        [HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult ShipmentOrderDetails([FromBody]ShipmentOrderRequest requestData)
        {
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId, requestData.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var response =
                _partialShipmentLogic.GetPartialShipmentOrderForResubmit(requestData.ReferenceId, requestData.MasterOrderNumber, requestData. UserAccountId);
            return Json(response);
        }

        #region Private Methods

        private bool IsAuthorized(long supportAccountId, long userAccountId)
        {
            bool isAuthorized = _securityLogic.IsTacUserOrAdmin(ApiConstants.PANW_SUPPORT_ACCOUNT_ID, userAccountId) || _securityLogic.IsSuperUser(supportAccountId, userAccountId);

            return isAuthorized;
        }
        #endregion
    }
}
