using System;
using Csp.Api.EntityClasses;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface ISupportAccountRepository {
        // Comments By LK On 05/10/2018
        // This method is still being used in CreateStubDevice. Please clean-up the code before marking it obsolete
        //[Obsolete("Shouldn't Guess - don't we know in all cases?")]
		long GuessUserAccountIdBySupportAccountId(long supportAccountId);
		void UpdateDeviceSupportAccountInCsp(long deviceId, long destinationSupportAccountId);

		string GetSupportAccountNameBySupportAccountId(long supportAccountId);

		Supportaccount GetSupportAccountById(long id);

		bool IsActiveSupportAccount(long supportAccountId);
		void RemoveDuplicateAccountAndFixSapMappings(long duplicateSupportAccountId, long keeperSupportAccountId);
		void ConvertEvalSupportAccountToCustomer(long supportAccountId);
		bool IsEvalSupportAccount(long supportAccountId);
		MatchingSupportAccount[] GetSupportAccountsBySfdcid(string sfdcid);

	    bool IsSupportAccountExists(string supportAccountName);
	    bool CreateSupportAccount(string name, int type, long createdBy, out long SupportAccountId);
        long? GetSupportAccountIdByName(string supportAccountName);

				bool CreateAwsAccountSupportAccountMapping(string awsAccountId, long supportAccountId);
				bool IsAwsAccount(long supportAccountId);
	    bool RegistrationRequiresSuApproval(long supportAccountId);
        void SetRegistrationRequiresSuApproval(long supportAccountId, bool registrationRequiresSuApproval);

	    bool IsCsspAccount(long supportAccountId);

        bool IsFedrampAccount(long supportAccountId);

        bool IsMSSPAccount(long supportAccountId);

        string GetSupportAccountNameForEndUser(string csspAccountName);
	    string GetDefaultSfdcAccountForCssp(long csspSupportAccountId);
        SupportAccount GetSeLabKitSupportAccountId(long userAccountId);

        SupportAccount CreateSeLabKitSupportAccount(long userAccountId, string salesforceAccountId, int accountTypeId, string accountName,
            string phoneNumber, string address1,
            string address2, string city, string region, string postalCode, long? countryId, long createdByUserAccountId);

        int CanImpersonate(long supportAccountId);
    }

}
