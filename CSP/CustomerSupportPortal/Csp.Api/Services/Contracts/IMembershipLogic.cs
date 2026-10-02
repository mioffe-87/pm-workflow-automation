using System;
using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts {
	public interface IMembershipLogic {
		string GetMembershipResponseGuidBId(long id);
		EntityClasses.Membership GetMembershipByResponseGuid(string id);
		void UpdateMembershipStatus(EntityClasses.Membership membership);
		string GetUserEmailByMembershipId(long id);
		string GetSupportAccountNameByMembershipId(long id);
		Membership CreateMembership(Membership membership, long groupSupportAccountId = 0);
	    void AssignMembershipRoles(long membershipId, long userAccountId, long supportAccountId, long createdByUserAccountId, bool assignSuperUser = false, bool allowedOrg = true);
		Membership GetMembership(long membershipId, long restrictedByUserAccountId);
        Membership UpdateMembership(Membership membership);

		void UpdateMembershipSf(MembershipModel membership);

		List<SupportAccountMembershipModel> GetSupportAccountMembers(long supportAccountId, long restrictedByUserAccountId,
                                                                                                            DataParameters parameters, out List<SupportAccountDomainModel> uniqueAccountDomainModels);

		byte[] GetSupportAccountMembersCsv(long supportAccountId, long restrictedByUserAccountId, string searchField, string searchValue);

		List<MembershipListItem> GetDeviceMembers(long deviceId, long restrictedByUserAccountId, DataParameters parameters);
		List<MembershipListItem> GetGroupMembers(long groupId, long restrictedByUserAccountId, DataParameters parameters);
		byte[] GetGroupMembersCsv(long groupId, long restrictedByUserAccountId, string searchField, string searchValue);
		void DeleteMembership(long membershipId, long deletedByUserAccountId);
		bool HasMembershipforSupportAccountByEmail(string email, long supportAccountId);
		bool HasMembershipforGroupByEmail(string email, long groupId);
		bool HasMembershipforDeviceByEmail(string email, long deviceId);
		void AddUpdateSalesforceContact(Membership membership, string lastUpdateByEmail, long lastUpdateByUserAccountId);
        void DeleteSalesforceContact(string email, long supportAccountId, long lastUpdateByUserAccountId);
        long? GetSupportAccountIdByGroupId(long groupId);
		bool HasAccessToSupportAccount(long supportAccountId, long userAccountId);
	    List<MembershipDetailForSfdcContactSync> GetAllMembershipsWithoutSfdcContact();
	    long GetNumberOfPendingMembershipsBySupportAccountId(long supportAccountId, List<RoleSecurity> roleSecurities);
	    IEnumerable<PendingMembershipModel> GetAllPendingMemberships(long supportAccountId);
        bool IsThreatResearcher(long userAccountId, long supportAccountId, out string errorMessage);
        MembershipDetails GetMembershipDetailsBySfdcPortalUsername(string portalUsername, bool hasMultipleAccounts = false);

        int GetCountOfUsersByRole(long supportAccountId, int roleId);
        string GetSalesforceContactIdByUserAndSupportAccountId(long userAccountId, long supportAccountId);
    }

	public class MembershipModel {
		public long MembershipId { get; set; }
		public string SalesforceContactId { get; set; }
		public string SalesforceLogInId { get; set; }
		public string JiveId { get; set; }
		public long LastUpdateByUserAccountId { get; set; }
	}

    public class PendingMembershipModel
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public DateTime CreateDate { get; set; }
        public string MembershipResponseGuid { get; set; }
    }
}
