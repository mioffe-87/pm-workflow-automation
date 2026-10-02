using Csp.Api.EntityClasses;
using System.Collections.Generic;
using Csp.Api.Logic;
using Csp.Api.LoginService;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {

	public interface IUserAccountLogic {

		bool IsValidNonPublicEmail(string email);
		UserAccount GetUserAccount(long userAccountId, long restrictedByUserAccountId);
		void UpdateTemporaryAnnouncement(UserAccount userAccount, long restrictedByUserAccountId);

		UserAccount GetUserAccountByEmail(string email);
		bool IsEmailExists(string email);
		bool IsDisplayNameExists(string displayName, long id);
		UserAccount UpdateUserAccount(UserAccount userAccount, long restrictedByUserAccountId, string clientIpAddress, string supportAccountName);
		UserAccountLogic.ValidateUserRegistrationModel ValidateUserRegistration(string email, string displayName, string deviceCode, string customerCode, bool showInternalErrors = false);
		UserProfile RegisterUserAccount(UserAccount userAccount, long supportAccountId, string password, string clientIpAddress, out long membershipId,out string errorMessage, out long userAccountId,
                                        bool isMembershipPending = false,bool isAwsRegistration = false, string supportAccountName = "", string cloudAccountId ="", long deviceId=0, 
                                        string deviceCode = "",string csspSerialNumber="", string customerAccountId ="", bool isUsingCloudDeviceForRegistration = false, 
                                        bool isAzureRegistration = false, bool isCsspRegistration =false, bool createMembership = true, bool isGoogleCloudRegistration = false, bool isPartialShipmentUser = false, bool allowedOrg = true);
		List<UserAccount> GetAdminUserAccountsBySupportAccountId(long supportAccountId);

		IEnumerable<WildfireUserAccountItem> GetWildfireUsersBySupportAccountId(long supportAccountId, DataParameters dataParameters);
		UserProfile AddUpdateWildfireUserAccount(WildfireUserAccount userAccount, long supportAccountId, List<long> deviceIds, string clientIpAddress,
				long createdByUserAccountId, out long wildfireUserAccountId, List<long> changedDeviceIds = null);
		void BulkUpdateWildfireUserAccountDevices(long wildfireUserAccountId, long[] deviceIds, long supportAccountId, string clientIp);
		void DeleteWildfireUserAccount(long wildfireUserAccountId, long supportAccountId, string clientIpAddress,
                string deletedByUserAccountId);
		Wildfireuseraccount GetWildfireUserAccountById(long id);
		bool IsWildfireUserAccountExists(string email, long supportAccountId);
		long GetWildfireUserAccountId(string email);
		void SetSelectedMembershipAndSalesForce(long userAccountId, long membershipId, long supportAccountId);
		bool ShowEmailToSuperUserOption(string deviceCode, string customerCode);
		void EmailSuperUsers(string email, string customerCode, string firstName, string lastName);
		bool IsEvalDeviceCode(string deviceCode);
		bool IsEvalSupportAccount(long supportAccountId);
		bool IsProfileExists(string email, string ipAddress);      

        void UpdateSalesforceContactsAccount(UserAccount userAccount, long? supportAccountId, string currentSfdcId,
			string newSfdcId);

		IEnumerable<UserAccount> GetAwsSupportAccountUsers(long supportAccountId);

        SelfRegistrationCodeModel IsValidSelfRegistrationCode(string code);

	    bool SupportAccountRequiresSuperUserApproval(long supportAccountId);

        IEnumerable<UserAccount> GetSupportAccountUsers(long supportAccountId);

        IEnumerable<NameValueDescription> GetSupportAccountUsersWithSfdcContactId(long supportAccountId, string searchTerm);

    	long GetDefaultSupportAccountByUser(long userAccountId);

	    NewUserDetails GetNewUserCreationDetails(long userAccountId, long supportAccountId, long membershipId);
	    bool GrantPermissionToUser(long supportAccountId, long userAccountId, Permission permission);
        List<LicensingApi> GetSupportAccountsByLicensingApiKeyExpiration();

	    void ProcessCloudServiceUsersTransfer();
	    bool IsUserExistsByUniqueIdAndEmail(string uniqueId, string email);
        UserAccount CreateUserAccount(UserAccount userAccount);
        void CreateJiveId(long userAccountId, string email);

        bool HasCPSPAdminRole(long userAccountId);

        UserProfile ActivateUser(string email);

        bool CanAddEmailToSupportAccount(long supportAccountId, string email);
        string GetDomainDetails(string domain);
        long GetCsspSupportAccountId(string customerAccountId);
    }
}
