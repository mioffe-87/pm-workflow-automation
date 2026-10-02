using System.Collections.Generic;

namespace Csp.Api.Services.Contracts {
	public interface IEvalAccountCreationRepository {
		bool SupportAccountExistsBySfAccountId(string sfAccountId);
		long CreateEvalAccount(string sfAccountId, string name, string phoneNumber, string addr1, string addr2, string city, string region, string postalCode, string country, string createdByEmail);
		void LogEvalAccountCreation(bool isEval, long? deviceId, long? licenseId, string evalId, string sfAccountId, long? supportAccountId);
		LatestIdsModel GetLatestInspectedIds();
		IEnumerable<EvalDeviceModel> GetDevicesCodesToProcess(LatestIdsModel latestInspectionIds);
		string[] GetVmsByAuthCode(string authCode);
		void LogVmEvalRequest(string deviceCode, string evalId, string salesforceAccountId);
		void LogEvalSfdcMappings(string evalId, string sfdcId, long supportAccountId);

        string GetSnByAuthCode(string authCode);

        IEnumerable<Model.SnAuthCode> GetSnsByAuthCodes(IEnumerable<string> authCodes);
        IEnumerable<TempEvalDeviceModel> GetDevicesCodesFromTempTable(int limit);
        void UpdateEvalTempRecord(bool isEval, long productId, string evalId, string sfAccountId, long? supportAccountId);
    }

	public class LatestIdsModel {
		public long? DeviceId { get; set; }
		public long? LicenseId { get; set; }
	}

	public class EvalDeviceModel {
		public long Id { get; set; }
		public string DeviceCode { get; set; }
		public string EvalId { get; set; }
		public EvalDeviceType Type { get; set; }
		public enum EvalDeviceType {
			Panorama,
			Vm,
            VM,
        }
	}    public class TempEvalDeviceModel : EvalDeviceModel
    {
        public string SfAccountId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Addr1 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}