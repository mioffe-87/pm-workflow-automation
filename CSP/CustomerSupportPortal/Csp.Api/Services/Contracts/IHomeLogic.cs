using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Security;
using LINQtoCSV;

namespace Csp.Api.Services.Contracts {
	public interface IHomeLogic {

		IEnumerable<ResourceLink> GetResourceLinks(bool hasPermViewSoftwareUpdates, bool hasPermViewDynamicUpdates, bool hasSupportAccountSelected, bool hasPermCaseManagement, bool canSeeAutoFocusLink, bool hasViewASCDashboardReport, bool hasreturnsPortalAccess, bool hasPartnerReportAccess, long UserAccountId, long supportAccountId);
		CurrentLoggedInData GetCurrentLoggedInData(long? supportAccountId, long restrictedByUserAccountId);
		IEnumerable<SelectableAccount> GetAvailableAccounts(long userAccountId, DataParameters dataParameters, bool setRoles);
        IEnumerable<SelectableAccount> GetAvailableAccountsForDevice(long userAccountId, DataParameters dataParameters, bool setRoles);
		byte[] GetAvailableAccountsCsv(long userAccountId, string searchField, string searchValue);
        byte[] GetAvailableDeviceAccountsCsv(long userAccountId, string searchField, string searchValue);
		void SetDefault(long supportAccountId, long userAccountId);
		long? GetOrCreateDefaultSupportAccountId(long userAccountId);
		bool UserHasAccessToAccount(long supportAccountId, long userAccountId, List<RoleSecurity> roleSecurities);
		int GetNumberOfAccountsUserCanAccess(long userAccountId);
	    bool HasPendingMemberships(long userAccountId);
        BannerMessage GetBannerMessage();
	    string GetSalesforceLoginId(long supportAccountId,long userAccountId);
        bool EnableDayOneConfig(long supportAccountId);
    }

	public class CurrentLoggedInData {

		public bool IsSupportAccountSelected {
			get { return !string.IsNullOrEmpty(CurrentAccount); }
		}

		public string CurrentAccount { get; set; }
		public IEnumerable<string> CurrentRoles { get; set; }

		public long? DefaultSupportAccountId { get; set; }
		public long? CurrentSupportAccountId { get; set; }
		public bool IsDefault {
			get {
				if (DefaultSupportAccountId.HasValue && CurrentSupportAccountId.HasValue)
					return DefaultSupportAccountId == CurrentSupportAccountId;
				else return false;
			}
		}
	}

	public class SelectableAccount : IEquatable<SelectableAccount> {
		public long SupportAccountId { get; set; }
		public string CompanyName { get; set; }
		public string Type { get; set; }
		public long AccountTypeId { get; set; }
		public string Roles { get; set; }
        public bool IsMfaEnabled { get; set; }
        public string MfaStatus
        {
            get { return IsMfaEnabled ? "Enabled" : "Disabled"; }
        }


		//TODO shouldn't the compare just be on support account id? - ML 20150108
		public bool Equals(SelectableAccount other) {
			return SupportAccountId == other.SupportAccountId
						 && CompanyName == other.CompanyName;
		}
	}

	public class SupportAccountCsv {
		[CsvColumn(FieldIndex = 1, Name = "Account Id")]
		public long SupportAccountId { get; set; }

		[CsvColumn(FieldIndex = 2, Name = "Account Name")]
		public string CompanyName { get; set; }

		[CsvColumn(FieldIndex = 3, Name = "Roles")]
		public string Roles { get; set; }     
	}
}
