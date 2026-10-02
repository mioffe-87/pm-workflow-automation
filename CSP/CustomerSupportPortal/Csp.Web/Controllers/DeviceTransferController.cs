using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using DeviceTransferBy = Csp.Web.DataTransferObjects.DeviceTransferBy;
using NotificationCategory = Csp.Api.Model.Enums.NotificationCategory;

namespace Csp.Web.Controllers {

	public class DeviceTransferController : BaseController {

		private readonly IDeviceTransferLogic _deviceTransferLogic;
        private readonly IEulaLogic _eulaLogic;

        public DeviceTransferController(IDeviceTransferLogic deviceTransferLogic, IEulaLogic eulaLogic)
        {
            _deviceTransferLogic = deviceTransferLogic;
            _eulaLogic = eulaLogic;
		}

		[HttpGet, CspAuthorize(Permission.BeginTransferDevice)]
		public ActionResult BeginTransfer(long deviceId) {

			var dbModel = _deviceTransferLogic.GetBeginTransferData(deviceId, User.UserAccount.Id);

			var model = Mapper.Map<BeginDeviceTransferDto>(dbModel);

			return PartialView("../Partial/_BeginDeviceTransfer", model);
		}

		[HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.BeginTransferDevice)]
		public async Task<ActionResult> BeginTransfer(BeginDeviceTransferDto model) {
			
            var originalDbModel = _deviceTransferLogic.GetBeginTransferData(model.DeviceId, User.UserAccount.Id);

			model.SerialNumber = originalDbModel.SerialNumber;
			model.DeviceName = originalDbModel.DeviceName;
			model.PartNumber = originalDbModel.PartNumber;

			//if (!ModelState.IsValid) {
			//	return PartialView("../Partial/_BeginDeviceTransfer", model);
			//}

			var dbModel = Mapper.Map<BeginDeviceTransferModel>(model);
			dbModel.SourceUserAccountId = User.UserAccount.Id;

			var errorMsg = await _deviceTransferLogic.BeginTransfer(dbModel);

			if (string.IsNullOrEmpty(errorMsg)) {
				if (model.TransferBy.Equals(DeviceTransferBy.SupportAccount))
                {
                    model.Notifications.Add(new ResponseNotification("The device is successfully transfered to support account: <a href=\"/SupportAccount/Devices/" + model.SupportAccountId + "?devicecode=" + model.SerialNumber + "\">" + model.SupportAccountName + "</a>", ResponseState.Information));
                }
				else
				{
                    model.Notifications.Add(new ResponseNotification("The device is now pending transfer until the recipient receives the device", ResponseState.Success));
				}
               
              
			} else {
				model.Notifications.Add(new ResponseNotification(errorMsg, ResponseState.Error));
			}

			return PartialView("../Partial/_BeginDeviceTransfer", model);
		}

	    [HttpGet] //CspAuthorize(Permission.ReviewAllDeviceTransfers)
	    public ActionResult ReviewTransfers()
	    {

	        var pendingTransfers = _deviceTransferLogic.GetReviewTransferData(User.UserAccount,
	        CurrentState.GetSupportAccountId().Value);
	        var pendingTransfersDto = Mapper.Map<IEnumerable<ReviewDeviceTransferModel>>(pendingTransfers);

	        var model = new ReviewDeviceTransferDto
	        {
	            DeviceTransfer = pendingTransfersDto,
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
	        model.Eula.Context = EulaContext.AcceptDeviceTransfer;

	        #endregion

	        return PartialView("../Partial/_ReviewDeviceTransfers", model);
	    }

	    [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ReviewTransfers(long deviceTransferId, bool isApproved, int? SelectedEulaId)
		{
		    var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;

            var result = _deviceTransferLogic.ReviewTransfer(deviceTransferId, isApproved, User.UserAccount, supportAccountId);

            var pendingTransfers = _deviceTransferLogic.GetReviewTransferData(User.UserAccount, supportAccountId);

			var pendingTransfersDto = Mapper.Map<IEnumerable<ReviewDeviceTransferModel>>(pendingTransfers);

			var model = new ReviewDeviceTransferDto {
				DeviceTransfer = pendingTransfersDto,
			};

			if (result.IsSuccess)
            {
                #region Store EULA Agreements
                if (SelectedEulaId.HasValue)
                {
                    try
                    {
                        var eulaContextId = EulaContext.AcceptDeviceTransfer;
                        var eulaId = SelectedEulaId.Value;
                        var signedBy = User.UserAccount.Email;
                        var deviceId = deviceTransferId;

                        var ea = new EulaAgreements()
                        {
                            DeviceId = deviceId,
                            SupportAccountId = supportAccountId,
                            EulaContextId = eulaContextId,
                            EulaId = eulaId,
                            SignedBy = signedBy
                        };

                        _eulaLogic.EndUserAgreements(ea);
                    }
                    catch (Exception)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while accepting device transfer for DeviceId {0}. Eula Id Selected is - {1}", deviceTransferId, SelectedEulaId.Value)));
                    }
                }
                #endregion
                var successMsg = isApproved ? "Device transfer successful" : "Device transfer rejected";

                SiteHelper.UpdateNotificationMessageStatus(entityId:deviceTransferId.ToString(),category:NotificationCategory.DeviceTransfer.ToDescription());
				model.Notifications.Add(new ResponseNotification(successMsg, ResponseState.Success));
			} else {

				model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
			}
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
            model.Eula.Context = EulaContext.AcceptDeviceTransfer;

            #endregion

            return PartialView("../Partial/_ReviewDeviceTransfers", model);
        }

	}
}
