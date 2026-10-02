using System.Collections.Generic;
using Csp.Api.Model;
using Newtonsoft.Json;

namespace Csp.Api.Services.Contracts {
	public interface ISupportAccountLogic {		
		string GetSalesforceIdBySupportAccountId(long id);
		SupportAccount GetSupportAccount(long supportAccountId, long restrictedByUserAccountId);
		EntityClasses.Supportaccount GetSupportAccountById(long id);
		string GetSupportAccountNameById(long id);
		List<NameValue> GetSupportAccountsBySapId(long id);
		SapAccountModel GetSapAccountDetails(string deviceCode);
		EvalToCustModel ValidateEvalAccountToCust(long supportAccountId, string deviceCode, string customerCode);
		EvalToCustModel ConvertEvalAccountToCust(long supportAccountId, string deviceCode, string authCode, long userAccountId);
		List<SupportAccountListItem> GetSupportAccountbyUserAccountId(long userAccountId, DataParameters parameters);
		SupportAccount UpdateSupportAccount(SupportAccount supportAccount, long restrictedByUserAccountId);
		AccountType GetAccountType(long accountTypeId);
		List<Role> GetsupportAccountDefaultRoles(long supportAccountId, DataParameters parameters);
		List<AccountType> GetAccountTypes(DataParameters dataParameters);
		bool AddUpdateSelectedSupportAccount(string email, long membershipId, long supportAccountId);
		List<SelectorAccountListItem> GetSupportAccountsAndGroupsByEmailId(string emailId);

	    List<SupportAccountMembershipListItem> GetSupportAccountMembershipsbyUserAccountId(long userAccountId,
	        long restrictedByUserAccountId,
	        DataParameters parameters);
		MatchingSupportAccount[] GetSupportAccountsBySfdcid(string sfdcid);

        bool IsSupportAccountExists(string supportAccountName);
	    long? GetSupportAccountIdByName(string supportAccountName);
        bool CreateSupportAccount(string name, int type, long createdBy, out long SupportAccountId);

		bool CreateAwsAccountSupportAccountMapping(string awsAccountId, long supportAccountId);
	    bool RegistrationRequiresSuApproval(long supportAccountId);
        void SetRegistrationRequiresSuApproval(long supportAccountId, bool registrationRequiresSuApproval);

        bool IsCsspAccount(long supportAccountId);
        bool IsFedrampAccount(long supportAccountId);
        bool IsMSSPAccount(long supportAccountId);

        SupportAccount GetSeLabKitSupportAccountId(long userAccountId);

        SupportAccount CreateSeLabKitSupportAccount(long userAccountId, string salesforceAccountId, int accountTypeId, string accountName,
            string phoneNumber, string address1, string address2, string city, string region, string postalCode, long? countryId, long createdByUserAccountId);

        bool IsThirdPartyIdpBetaAccount(string domain);
        int CanImpersonate(long supportAccountId);
    }

	public class SapAccountModel {
		public long SupportAccountId { get; set; }
		public string CompanyName { get; set; }
		public bool IsActive { get; set; }
	}

	public class EvalToCustModel {
		public bool IsActiveOtherAccount { get; set; }
		public string CompanyName { get; set; }
		public string Error { get; set; }
		public long OtherSupportAccountId { get; set; }
	}

	public class MatchingSupportAccount {
		public long SupportAccountId { get; set; }
		public string Name { get; set; }
		public int NumberOfMembers { get; set; }
		public int NumberOfAssets { get; set; }
	}

    public class PendoAccountDetails
    {
        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
    public class PortalContextDetails
    {
        [JsonProperty(PropertyName = "organization")]
        public string Organization { get; set; }
        [JsonProperty(PropertyName = "searchHub")]
        public string SearchHub { get; set; }
        [JsonProperty(PropertyName = "searchPageUrl")]
        public string SearchPageUrl { get; set; }
        [JsonProperty(PropertyName = "searchToken")]
        public string SearchToken { get; set; }
        [JsonProperty(PropertyName = "searchDataSource")]
        public string SearchDataSource { get; set; }
        [JsonProperty(PropertyName = "selfHelpDataSource")]
        public string SelfHelpDataSource { get; set; }
        [JsonProperty(PropertyName = "restEndpointUrl")]
        public string RestEndpointUrl { get; set; }
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }
        [JsonProperty(PropertyName = "supportAccountId")]
        public string SupportAccountId { get; set; }
        [JsonProperty(PropertyName = "supportAccountName")]
        public string SupportAccountName { get; set; }
        [JsonProperty(PropertyName = "hasMultipleAccounts")]
        public bool HasMultipleAccounts { get; set; }
        [JsonProperty(PropertyName = "resourceRevisionNumber")]
        public string ResourceRevisionNumber { get; set; }
        [JsonProperty(PropertyName = "userTypeKey")]
        public string UserTypeKey { get; set; }
        [JsonProperty(PropertyName = "userType")]
        public string UserType { get; set; }
        [JsonProperty(PropertyName = "envType")]
        public string EnvType { get; set; }
        [JsonProperty(PropertyName = "roles")]
        public string Roles { get; set; }
        [JsonProperty(PropertyName = "userAccountId")]
        public string UserAccountId { get; set; }
        [JsonProperty(PropertyName = "isAuthUser")]
        public bool IsAuthUser { get; set; }
        [JsonProperty(PropertyName = "pendoKey")]
        public string PendoKey { get; set; }
        [JsonProperty(PropertyName = "isAscUser")]
        public bool IsAscUser { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
        [JsonProperty(PropertyName = "impersonationSetting")]
        public int ImpersonationSetting { get; set; }

        [JsonProperty(PropertyName = "userTheme")]
        public string UserTheme { get; set; }

        [JsonProperty(PropertyName = "isAiEligible")]
        public bool IsAiEligible { get; set; }

        [JsonProperty(PropertyName = "aiAgentImpersonation")]
        public bool AiAgentImpersonation { get; set; }

        [JsonProperty(PropertyName = "aiAgentDeclineReason")]
        public string AiAgentDeclineReason { get; set; }

        [JsonProperty(PropertyName = "cspEndpointUrl")]

        public string cspEndpointUrl { get; set; }
    }

    public class SeLabKitAccountModel
    {
        public string Email { get; set; }
        public string Key { get; set; }
    }

    public class SeLabKitAccountDetails
    {
        public long AccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
