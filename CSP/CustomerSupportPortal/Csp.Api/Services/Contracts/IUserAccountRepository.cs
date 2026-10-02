using System;
using System.Collections.Generic;
using Csp.Api.EntityClasses;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface IUserAccountRepository {
		IEnumerable<Useraccount> GetAdminUserAccountsBySupportAccountId(long supportAccountId);
		IEnumerable<WildfireUserAccountItem> GetWildfireUsersBySupportAccountId(long supportAccountId, DataParameters parameters);
		bool IsWildfireUserAccountExists(string email, long supportAccountId);
		long GetWildfireUserAccountId(string email);
		long AddUpdateWildfireUserAccount(AddUpdateWildfireUserAccountModel userAccount, long supportAccountId, List<long> deviceIds, string createdByUser, List<long> changedDeviceIds = null, string clientIpAddress =" ");
		void DeleteWildfireUserAccount(long wildfireUserAccountId, long supportAccountId, string clientIpAddress,
                string deletedByUserAccountId);
		void SetDefaultAccount(long supportAccountId, long userAccountId);
		void AssignDefaultAccount(long supportAccountId, long userAccountId);
		long? GetDefaultAccount(long userAccountId);
		void SetJiveId(long userAccountId, string jiveId);
		void SetSelectedSalesfoceData(long userAccountId, SelectedSalesForceData data);
		IEnumerable<UserAccount> GetAwsSupportAccountUsers(long supportAccountId);
        IEnumerable<UserAccount> GetSupportAccountUsers(long supportAccountId);

        IEnumerable<NameValueDescription> GetSupportAccountUsersWithSfdcContactId(long supportAccountId, string searchTerm);
	    bool GrantPermissionToUser(long supportAccountId, long userAccountId, Permission permission);
	}

	public class WildfireUserAccountItem {
		public long WildfireUserAccountId { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? CreatedOn { get; set; }
	}

	public class WildfireUserAccountItemComparator : IEqualityComparer<WildfireUserAccountItem> {
		public bool Equals(WildfireUserAccountItem x, WildfireUserAccountItem y) {
			return x.WildfireUserAccountId.Equals(y.WildfireUserAccountId);
		}

		public int GetHashCode(WildfireUserAccountItem obj) {
			return obj.WildfireUserAccountId.GetHashCode();
		}
	}

	public class AddUpdateWildfireUserAccountModel {
		public long? WildfireUserAccountId { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string CreatedByUserEmail { get; set; }
		public long LastUpdatedByUserAccountId { get; set; }
	}
}
