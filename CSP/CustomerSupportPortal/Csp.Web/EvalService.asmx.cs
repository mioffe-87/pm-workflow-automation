using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Csp.Api.Logic;
using Csp.Api.Logic.Salesforce;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.Persistence;
using Csp.Infrastructure.Data.Persistence.Configuration;
using PAN.Common.Infrastructure.Data.Persistence;
using System.Configuration;
using Csp.Api.Model;

namespace Csp.Web {
	/// <summary>
	/// Summary description for EvalService
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class EvalService : System.Web.Services.WebService {

		private string _privateKey = ConfigurationManager.AppSettings["EncryptionProviderPrivateKey"];

		private readonly IEvalAccountCreationService _evalAccountCreationService;
		private readonly IEncryptionProvider _encryptionProvider;
		private readonly IUserAccountLogic _userAccountLogic;
		private readonly ISupportAccountLogic _supportAccountLogic;
		private readonly ILoginServiceLogic _loginServiceLogic;
	    private readonly ISalesforceService _salesforceService;

		public EvalService()
		{
            _salesforceService = new SalesforceThrottler(new SalesforceLogsRepository(), new LookupRepository());
			_loginServiceLogic = new LoginServiceLogic();
            _evalAccountCreationService = new EvalAccountCreationService(new EvalAccountCreationRepository(), _salesforceService);
			_encryptionProvider = new EncryptionProvider();
            _userAccountLogic = new UserAccountLogic(null, null, null, null, _salesforceService, null, new UserRegistrationRepository(), new EvalTrackerRepository(), null, null, null, _encryptionProvider, new LookupRepository(), _loginServiceLogic, null, null, null, null, null);
			_supportAccountLogic = new SupportAccountLogic(null, null, null, null, new SupportAccountRepository(null, null, null), null, null);
		}

		[WebMethod]
		public long CreateSupportAccount(string sfAccountId, string key, string name,
			string phoneNumber, string addr1, string addr2, string city, string region,
			string postalCode, string country, string createdByEmail, string evalId, string supportAccountId) {

			try {

				var isKeyValid = _encryptionProvider.IsKeyValid(sfAccountId, key, _privateKey);
				if (!isKeyValid) return 0;

				long? parsedSupportAccountId = null;
				if (!string.IsNullOrEmpty(supportAccountId)) {
					long temp;
					if (long.TryParse(supportAccountId, out temp)) {
						parsedSupportAccountId = temp;
					}
				}


				var createdSupportAccountId = _evalAccountCreationService.CreateEvalAccount(sfAccountId, name, phoneNumber, addr1, addr2,
																																						 city, region,
																																						 postalCode, country, createdByEmail, evalId, parsedSupportAccountId);
				return createdSupportAccountId;

			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error in call to CreateSupportAccount(sfAccountId: {0}, key: {1}, name: {2}, phone: {3}, addr1: {4}, addr2: {5}, city: {6}, region: {7}, postalCode: {8}, country: {9}, createdByEmail: {10});",
					sfAccountId, key, name,
					phoneNumber, addr1, addr2, city, region,
					postalCode, country, createdByEmail)));
				throw;
			}
		}

		[WebMethod]
		public string[] GetVmsByAuthCode(string authCode, string key) {

			try {
				var isKeyValid = _encryptionProvider.IsKeyValid(authCode, key, _privateKey);
				if (!isKeyValid) return null;


				var vmSerialNumbers = _evalAccountCreationService.GetVmsByAuthCode(authCode);

				return vmSerialNumbers;

			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error in call to GetVmsByAuthCode(authcode: {0}, key: {1});",
					authCode, key)));
				throw;
			}
		}

		[WebMethod]
		public string[] GetTrapsByAuthCode(string authCode, string key) {

			try {
				var isKeyValid = _encryptionProvider.IsKeyValid(authCode, key, _privateKey);
				if (!isKeyValid) return null;


				var vmSerialNumbers = _evalAccountCreationService.GetVmsByAuthCode(authCode);

				return vmSerialNumbers;

			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error in call to GetVmsByAuthCode(authcode: {0}, key: {1});",
					authCode, key)));
				throw;
			}
		}

		[WebMethod]
		public string ProcessEval(string deviceCode, string key, long? deviceId, long? licenseId, string evalId) {

			var isKeyValid = _encryptionProvider.IsKeyValid(deviceCode, key, _privateKey);
			if (!isKeyValid) return null;

			if (deviceId.HasValue == licenseId.HasValue) {
				return "invalid input";
			}

			var evalDeviceModel = new EvalDeviceModel { DeviceCode = deviceCode, EvalId = evalId };

			if (deviceId.HasValue) {
				evalDeviceModel.Id = deviceId.Value;
				evalDeviceModel.Type = EvalDeviceModel.EvalDeviceType.Panorama;
			} else {
				evalDeviceModel.Id = licenseId.Value;
				evalDeviceModel.Type = EvalDeviceModel.EvalDeviceType.Vm;
			}

			_evalAccountCreationService.ProcessEvalDeviceCode(evalDeviceModel);

			return "Run complete";
		}

		[WebMethod]
		public string ProcessCirroEval(string deviceCode, string key, long deviceId, string evalId) {
			try {
				return ProcessEval(deviceCode, key, deviceId, null, evalId);
			} catch (Exception oex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
				return "Error Occured " + oex.ToString();
			}
		}

		[WebMethod]
		public string ValidateSelfServiceRegistration(string email, string displayName, string deviceCode,
																															 string customerCode, string key) {

			var isKeyValid = _encryptionProvider.IsKeyValid(email + displayName + deviceCode + customerCode, key, _privateKey);
			if (!isKeyValid) return null;

			var validationResults = _userAccountLogic.ValidateUserRegistration(email, displayName, deviceCode, customerCode,
																												showInternalErrors: true);

			if (string.IsNullOrEmpty(validationResults.ErrorMessage)) {
				return "Success - Access to SupportAccountID: " + validationResults.SupportAccountId;
			} else {
				return validationResults.ErrorMessage;
			}
		}

		[WebMethod]
		public MatchingSupportAccount[] GetSupportAccountsBySfdcid(string sfdcid, string key) {

			var isKeyValid = _encryptionProvider.IsKeyValid(sfdcid, key, _privateKey);
			if (!isKeyValid) return null;

			return _supportAccountLogic.GetSupportAccountsBySfdcid(sfdcid);
		}

        [WebMethod]
        public string GetSnByAuthCode(string authCode, string key)
        {
            try
            {
                var isKeyValid = _encryptionProvider.IsKeyValid(authCode, key, _privateKey);
                if (!isKeyValid)
                {
                    return null;
                }

                return _evalAccountCreationService.GetSnByAuthCode(authCode);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error in call to GetSnByAuthCode(authcode: {0}, key: {1});", authCode, key)));
                throw;
            }
        }

        [WebMethod]

        public SnAuthCode[] GetSnsByAuthCodes(string[] authCodes, string key)
        {
            string authCodesFlattened = string.Join(",", authCodes ?? new string[0]);
            try
            {
                var isKeyValid = _encryptionProvider.IsKeyValid(authCodesFlattened, key, _privateKey);
                if (!isKeyValid)
                {
                    return null;
                }

                return _evalAccountCreationService.GetSnsByAuthCodes(authCodes)?.ToArray();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error in call to GetSnsByAuthCodes(authcodes: {0}, key: {1});", authCodesFlattened, key)));
                throw;
            }
        }
    }
}
