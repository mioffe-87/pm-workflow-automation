using System.Collections;
using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {

	public interface IMembershipRepository {

		IEnumerable<string> GetUserRoles(long supportAccountId, long userAccountId);
		long? GetSingleSupportAccountMembership(long userAccountId);
		long GetSupportAccountIdFromMembership(long membershipId);
		IEnumerable<MembershipForSa> GetMembershipsForSupportAccountId(long userAccountId, long supportAccountId);
		SelectedSalesForceData GetSelectedSalesforceDataByMembership(long membershipId);
        IEnumerable<SfContact> GetUniqueContactsForSfUpdate(long userAccountId);

        IEnumerable<MembershipDetailForSfdcContactSync> GetAllMembershipsWithoutSfdcContact();
	    bool HasPendingMemberships(long userAccountId);
	    long GetNumberOfPendingMembershipsBySupportAccountId(long supportAccountId);
	    IEnumerable<PendingMembershipModel> GetAllPendingMemberships(long supportAccountId);
        MembershipDetails GetMembershipDetailsBySfdcPortalUsername(string portalUsername, bool hasMultipleAccounts = false);
        string GetSalesforceContactIdByUserAndSupportAccountId(long userAccountId, long supportAccountId);
    }

    public class SfContact
    {
        public long SupportAccountId { get; set; }
        public string SalesforceId { get; set; }
        public long MembershipId { get; set; }
    }

	public class SelectedSalesForceData {
		public long MembershipId { get; set; }
		public string SalesForceAccountId { get; set; }
		public string SalesForceContactId { get; set; }
		public string SalesForceLoginId { get; set; }
	}
}
