using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Castle.Core.Internal;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.UI.Fluent;

namespace Csp.Web.Controllers
{
    [Authorize]
    public class ApproveController : BaseController
    {
        private readonly IDeviceTransferLogic _deviceTransferLogic;
        private readonly IEulaLogic _eulaLogic;
        private readonly IVirtualMachineLogic _virtualMachineLogic;
        private readonly ILicenseLogic _licenseLogic;

        public ApproveController(IDeviceTransferLogic deviceTransferLogic, IEulaLogic eulaLogic, IVirtualMachineLogic virtualMachineLogic, ILicenseLogic licenseLogic)
        {
            _deviceTransferLogic = deviceTransferLogic;
            _eulaLogic = eulaLogic;
            _virtualMachineLogic = virtualMachineLogic;
            _licenseLogic = licenseLogic;
        }

        [HttpGet]
				[CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult Review(long entityId, string category, string notificationId)
        {
            var model = new ReviewRequestDto()
            {
                DeviceTransferRequest = new List<ReviewDeviceTransferModel>(),
                VmElaTokenRequest = new List<ElaTokensGrantModel>()
            };

           if (!string.IsNullOrEmpty(category) && entityId > 0)
            {
                var supportAccountId = CurrentState.GetSupportAccountId().HasValue
                    ? CurrentState.GetSupportAccountId().Value
                    : 0;

                model.Category = category.ToUpper();
                model.NotificationId = notificationId;

                #region Bind Eula to Model

                model.Eula = new EulaModel()
                {
                    Languages = _eulaLogic.GetEulaLanguages(),
                    Eulas = _eulaLogic.GetEula(),
                    DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                    SelectedLanguage = EulaLanguage.English
                };
                model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
                model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
                

                #endregion


								if (category.Equals(Csp.Api.Model.Enums.NotificationCategory.DeviceTransfer.ToDescription(), StringComparison.OrdinalIgnoreCase))
                {
                    model.Eula.Context = EulaContext.AcceptDeviceTransfer;

                    //get transfer detail
                    var pendingTransfers = _deviceTransferLogic.GetReviewTransferData(User.UserAccount,supportAccountId,entityId);
                    IEnumerable<ReviewDeviceTransferModel> pendingTransfersDto = Mapper.Map<IEnumerable<ReviewDeviceTransferModel>>(pendingTransfers);

                    if (pendingTransfersDto.Any())
                        model.DeviceTransferRequest = pendingTransfersDto;


								} else if (category.Equals(Csp.Api.Model.Enums.NotificationCategory.VMELATokenGrant.ToDescription(), StringComparison.OrdinalIgnoreCase))
                {
                    model.Eula.Context = EulaContext.ActivateVmAuthCode;
                   
                    //Check if user has access to accept token
                    var pendingGrants = _virtualMachineLogic.GetPendingElaTokensByUser(EVmType.Vm, supportAccountId, User.UserAccount.Id, entityId);
                    IEnumerable<ElaTokensGrantModel> pendingGrantsDto = Mapper.Map<IEnumerable<ElaTokensGrantModel>>(pendingGrants);


                    if (pendingGrants.Any())
                      model.VmElaTokenRequest = pendingGrantsDto;
                }

                if (model.VmElaTokenRequest == null && model.DeviceTransferRequest == null)
                {
                    model.Notifications.Add(new ResponseNotification("The decision was already submitted and the request is no longer valid.", ResponseState.Information));
                }

            }
            return PartialView("../Partial/_Approval", model);
        }

        [HttpPost, ValidateJsonAntiForgeryToken]
				[CspAuthorize(Permission.ViewSupportAccount)]
        public async Task<ActionResult> Review(long? deviceTransferId , bool isApproved, int? SelectedEulaId, string authCode, DateTime? grantDate, string grantedBy, string category,string notificationId)
        {
            var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
            var model = new ReviewRequestDto()
            {
                DeviceTransferRequest = new List<ReviewDeviceTransferModel>(),
                VmElaTokenRequest = new List<ElaTokensGrantModel>()
            };

            var isSuccess = false;

           try
            {
                var eulaId = SelectedEulaId.Value;
                var signedBy = User.UserAccount.Email;

                var ea = new EulaAgreements()
                {
                    SupportAccountId = supportAccountId,
                    EulaId = eulaId,
                    SignedBy = signedBy
                };

                #region Bind Eula to Model

                model.Eula = new EulaModel()
                {
                    Languages = _eulaLogic.GetEulaLanguages(),
                    Eulas = _eulaLogic.GetEula(),
                    DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                    SelectedLanguage = EulaLanguage.English
                };
                model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
                model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;


                #endregion

                //Return if device transfer id and authcode is blank
                if ((deviceTransferId == null || deviceTransferId <= 0) && string.IsNullOrEmpty(authCode))
                {
                    model.Notifications.Add(new ResponseNotification("An error occured. Please contact support.", ResponseState.Error));

                    return this.Json(model);
                }

								if (category == Csp.Api.Model.Enums.NotificationCategory.DeviceTransfer.ToDescription())
                {
                    model.Eula.Context = EulaContext.AcceptDeviceTransfer;
										model.Category = Csp.Api.Model.Enums.NotificationCategory.DeviceTransfer.ToDescription();

                    ea.EulaContextId = EulaContext.AcceptDeviceTransfer;
                    ea.DeviceId = deviceTransferId;

                    var result = _deviceTransferLogic.ReviewTransfer(Convert.ToInt64(deviceTransferId), isApproved, User.UserAccount, supportAccountId);

                    if (result.IsSuccess)
                    {
                        isSuccess = true;
                        var successMsg = isApproved ? "Device Transfer Successful" : "Device Transfer Rejected";
                        model.Notifications.Add(new ResponseNotification(successMsg, ResponseState.Success));

                        if(isApproved)
                          _eulaLogic.EndUserAgreements(ea);
                    }
                    else
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(result.ErrorMessage));
                        model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
                    }

								} else if (category == Csp.Api.Model.Enums.NotificationCategory.VMELATokenGrant.ToDescription())
                {
                    model.Eula.Context = EulaContext.ActivateVmAuthCode;
										model.Category = Csp.Api.Model.Enums.NotificationCategory.VMELATokenGrant.ToDescription();

                    License licensedetail = _licenseLogic.GetLicenseByAuthcode(authCode);

                    ea.EulaContextId = EulaContext.ActivateVmAuthCode;
                    ea.LicenseId = licensedetail.Id ;

                    var elaTokenModel = new ElaTokensGrantModel
                    {
                        AuthorizationCode = authCode,
                        SupportAccountId = supportAccountId,
                        GrantDate = grantDate == null? DateTime.MinValue : Convert.ToDateTime(grantDate),
                        SourceUser = grantedBy
                    };

                    string result = await _virtualMachineLogic.AcceptElaTokenGrant(EVmType.Vm, elaTokenModel, isApproved, User.UserAccount.Id);
                    if (string.IsNullOrEmpty(result))
                    {
                        isSuccess = true;
                        var successMsg = "Submitted Successfully";
                        model.Notifications.Add(new ResponseNotification(successMsg, ResponseState.Success));

                         if(isApproved)
                           _eulaLogic.EndUserAgreements(ea);
                    }
                    else
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(result));
                        model.Notifications.Add(new ResponseNotification(result, ResponseState.Error));
                    }
                }

                if (isSuccess)
                {
                    SiteHelper.UpdateNotificationMessageStatus(notificationId);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(model);
        }
    }
}
