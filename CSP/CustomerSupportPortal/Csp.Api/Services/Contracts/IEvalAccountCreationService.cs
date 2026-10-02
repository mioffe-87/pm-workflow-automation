using System.Collections.Generic;

namespace Csp.Api.Services.Contracts {
	public interface IEvalAccountCreationService {
		void CreateEvalAccountsForVmEvals();
		long CreateEvalAccount(string sfAccountId, string name, string phoneNumber, string addr1, string addr2, string city, string region, string postalCode, string country, string createdByEmail, string evalId, long? supportAccountId);
		string[] GetVmsByAuthCode(string authCode);

		void ProcessEvalDeviceCode(EvalDeviceModel device);

        string GetSnByAuthCode(string authCode);

        IEnumerable<Model.SnAuthCode> GetSnsByAuthCodes(IEnumerable<string> authCodes);
        void CreateDeltaEvalAccountsForVmEvals(int limit);
        void ProcessEvalDeltaDeviceCode(TempEvalDeviceModel device);
    }
}
