using System;
using System.Web.Http;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;

namespace Csp.Web.Controllers {

	public class EvalRequestController : ApiController {

		private readonly IUserAccountLogic _userAccountLogic;
		private readonly IDeviceLogic _deviceLogic;
		private readonly ISupportAccountLogic _supportAccountLogic;
		private readonly IUserProfileDataAccess _userProfileDataAccess;

		public EvalRequestController(IUserAccountLogic userAccountLogic, IDeviceLogic deviceLogic, ISupportAccountLogic supportAccountLogic, IUserProfileDataAccess userProfileDataAccess) {
			_userAccountLogic = userAccountLogic;
			_deviceLogic = deviceLogic;
			_supportAccountLogic = supportAccountLogic;
			_userProfileDataAccess = userProfileDataAccess;
		}

		// This is obsolete and is no longer valid
		/*
		[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
		public long? CreateEvalAccount(string evalId, string key, string sfAccountId, string deviceCodes, string companyName, string addr1, string addr2, string city, string country, string regionState, string postalCode, string phone) {
			return 245;
		}

		[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
		public long? AddDeviceCodesToEval(string evalId, string key, string devicecodes) {
			return 245;
		}*/

		[HttpGet]
		[JwtTokenAuthentication]
		public EvalToCustModel ConvertEvalToCust(string email, long supportAccountId, string deviceCode, string customerCode) {

			if (!JwtTokenManager.IsAuthorized(email)) {
				return new EvalToCustModel { Error = Constants.UnauthorizedMessage };
			}

			var userProfile = _userProfileDataAccess.GetUserData(email);
			var userAccountId = userProfile.UserAccountId.HasValue ? userProfile.UserAccountId.Value : 0;
			var userSupportAccount = _supportAccountLogic.GetSupportAccount(supportAccountId, userAccountId);

			if (supportAccountId != 0 && userSupportAccount.Id != supportAccountId) {
				return new EvalToCustModel { Error = Constants.UnauthorizedMessage };
			}
			
			return _supportAccountLogic.ValidateEvalAccountToCust(supportAccountId, deviceCode, customerCode);
		}
	}
}
