using System.Text.RegularExpressions;
using Csp.Api.Services.Contracts;
using System.Collections.Generic;

namespace Csp.Api.Logic {
	public class EvalAccountCreationService : IEvalAccountCreationService {

		private readonly IEvalAccountCreationRepository _evalAccountCreationRepository;
		private readonly ISalesforceService _salesforceService;

		public EvalAccountCreationService(IEvalAccountCreationRepository evalAccountCreationRepository, ISalesforceService salesforceService) {
			_evalAccountCreationRepository = evalAccountCreationRepository;
			_salesforceService = salesforceService;
		}

		private readonly Regex _isEvalCode = new Regex(@"^(E\d+|EVAL-\d+)$");

		public void CreateEvalAccountsForVmEvals() {

			//Check the logs for the last ID we've used
			var latestInspectionIds = _evalAccountCreationRepository.GetLatestInspectedIds();

			//Get devs/lics since the last time we checked
			var deviceCodesToProcess = _evalAccountCreationRepository.GetDevicesCodesToProcess(latestInspectionIds);

			//Loop through each and query SF by the EVAL ID
			foreach (var device in deviceCodesToProcess) {

				ProcessEvalDeviceCode(device);
			}
		}

		public void ProcessEvalDeviceCode(EvalDeviceModel device) {
			bool isEval = false;
			string sfAccountId = string.Empty;
			long? supportAccountId = null;

			if (!string.IsNullOrEmpty(device.EvalId) && _isEvalCode.IsMatch(device.EvalId)) {
				//If the sf account id doesn't exist, then create the support account by calling
				SalesforceEvalData salesforceData = _salesforceService.GetSalesforceEvalDataByEvalId(device.EvalId);

				if (salesforceData != null) {

					isEval = true;
					sfAccountId = salesforceData.SfAccountId;

					//Log to vm evals request for self-service registration
					_evalAccountCreationRepository.LogVmEvalRequest(device.DeviceCode, device.EvalId, salesforceData.SfAccountId);

					//Do any companies use the sfaccountid
					var exists = _evalAccountCreationRepository.SupportAccountExistsBySfAccountId(salesforceData.SfAccountId);

					if (!exists) {
						//If not, then create the company AddUpdateEvalSupportAccount
						supportAccountId = _evalAccountCreationRepository.CreateEvalAccount(salesforceData.SfAccountId, salesforceData.Name,
																														 salesforceData.PhoneNumber, salesforceData.Addr1, string.Empty,
																														 salesforceData.City, salesforceData.Region,
																														 salesforceData.PostalCode, salesforceData.Country, null);
					}
				}
			}

			//Save to DB
			long? deviceId, licenseId;
			if (device.Type == EvalDeviceModel.EvalDeviceType.Panorama) {
				deviceId = device.Id;
				licenseId = null;
			} else {
				deviceId = null;
				licenseId = device.Id;
			}

			_evalAccountCreationRepository.LogEvalAccountCreation(isEval, deviceId, licenseId, device.EvalId, sfAccountId, supportAccountId);
		}

		public long CreateEvalAccount(string sfAccountId, string name, string phoneNumber, string addr1, string addr2, string city,
																	string region, string postalCode, string country, string createdByEmail, string evalId, long? supportAccountId) {

			var newSupportAccountId = _evalAccountCreationRepository.CreateEvalAccount(sfAccountId, name, phoneNumber, addr1, addr2, city, region,
																															postalCode, country, createdByEmail);

			if (supportAccountId.HasValue) {
				_evalAccountCreationRepository.LogEvalSfdcMappings(evalId, sfAccountId, supportAccountId.Value);

				return supportAccountId.Value;
			}

			return newSupportAccountId;
		}

		public string[] GetVmsByAuthCode(string authCode) {

			return _evalAccountCreationRepository.GetVmsByAuthCode(authCode);
		}


        public string GetSnByAuthCode(string authCode)
        {
            return _evalAccountCreationRepository.GetSnByAuthCode(authCode);
        }

        public IEnumerable<Model.SnAuthCode> GetSnsByAuthCodes(IEnumerable<string> authCodes)
        {
            return _evalAccountCreationRepository.GetSnsByAuthCodes(authCodes);
        }

        public void CreateDeltaEvalAccountsForVmEvals(int limit)
        {

            //Get devs/lics from the temp table
            var deviceCodesToProcess = _evalAccountCreationRepository.GetDevicesCodesFromTempTable(limit);

            //Loop through each and query SF by the EVAL ID
            foreach (var device in deviceCodesToProcess)
            {

                ProcessEvalDeltaDeviceCode(device);
            }
        }


        public void ProcessEvalDeltaDeviceCode(TempEvalDeviceModel device)
        {
            bool isEval = false;
            bool exists = false;
            string sfAccountId = string.Empty;
            long? supportAccountId = null;

            if (!string.IsNullOrEmpty(device.EvalId) && _isEvalCode.IsMatch(device.EvalId))
            {
                if (device.SfAccountId != null)
                {
                    isEval = true;
                    //Log to vm evals request for self-service registration
                    _evalAccountCreationRepository.LogVmEvalRequest(device.DeviceCode, device.EvalId, device.SfAccountId);

                    //Do any companies use the sfaccountid
                    exists = _evalAccountCreationRepository.SupportAccountExistsBySfAccountId(device.SfAccountId);

                    if (!exists)
                    {
                        //If not, then create the company AddUpdateEvalSupportAccount
                        supportAccountId = _evalAccountCreationRepository.CreateEvalAccount(device.SfAccountId, device.Name,
                                                                                                                       device.PhoneNumber, device.Addr1, string.Empty,
                                                                                                                         device.City, device.Region,
                                                                                                                         device.PostalCode, device.Country, null);
                    }
                }
            }

            //Save to DB
            long? deviceId, licenseId;
            if (device.Type == EvalDeviceModel.EvalDeviceType.Panorama)
            {
                deviceId = device.Id;
                licenseId = null;
            }
            else
            {
                deviceId = null;
                licenseId = device.Id;
            }

            _evalAccountCreationRepository.LogEvalAccountCreation(isEval, deviceId, licenseId, device.EvalId, device.SfAccountId, supportAccountId);
            // Update the Created flag in temp table
            _evalAccountCreationRepository.UpdateEvalTempRecord(isEval, device.Id, device.EvalId, device.SfAccountId, supportAccountId);
        }
    }
}
