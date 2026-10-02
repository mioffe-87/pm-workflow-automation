using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface ISalesforceService {
		AscContactInfo GetAscContactInfo(string id);

        SalesforceUserModel AddUpdateSalesforceContact(UserAccount userAccount, long? supportAccountId, string salesforceId, long membershipId,
                                                                        long createdByUserAccountId, bool isCsspDefaultAccount = false);

		SalesforceEvalData GetSalesforceEvalDataByEvalId(string evalId);

		void UpdateSalesforceContactsAccount(UserAccount userAccount, long? supportAccountId, string currentSfdcId,
			string newSfdcId);
        void DeleteSalesforceContactsAccount(string email, long supportAccountId);

        bool getSfAccountTypeByEmail(string email);
	}

	public class SalesforceEvalData {
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
