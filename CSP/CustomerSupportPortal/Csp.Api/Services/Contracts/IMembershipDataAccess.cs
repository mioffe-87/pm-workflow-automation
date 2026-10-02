﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Csp.Api.HelperClasses;
 using Csp.Api.Model;
 using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts {
	public interface IMembershipDataAccess {
		void UpdateMembershipSf(MembershipModel membershipModel);
		List<SupportAccountMembershipModel> ReadSupportAccountMembers(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters);

        int GetCountOfUsersByRole(long supportAccountId, int roleId);
	}

	public class SupportAccountMembershipModel {

		public long MembershipId { get; set; }

		public long UserAccountId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string DisplayName { get; set; }

		public string Email {
			get {
				if (_email != null) {
					return _email.Trim();
				}
				return _email;
			}
			set { _email = value; }
		}
		private string _email;

		public DateTime? ExpirationDate { get; set; }

		public DateTime ActivationDate { get; set; }

		public String Description { get; set; }

		public string RoleNames {
			get {
				return string.Join(",",
													 from r in Roles
													 select r.Name + "|" + r.Value);
			}
		}

		public IEnumerable<NameValue> Roles { get; set; }

        public bool IsMfaEnabled { get; set; }

        public bool IsMfaRequired { get; set; }

	    public string MfaStatus
	    {
            get { return MultiFactorAuthType.ToDescription(); }
        }

	    public MultiFactorAuthType MultiFactorAuthType { get; set; }

        public bool EnableEmailMfa { get; set; }

	    #region AutoFocus Trial related properties

        public bool AutoFocusTrialUser { get; set; }

        public DateTime? AutoFocusTrialExpirationDate { get; set; }

        #endregion

	    public string PreviousRoles { get; set; }

	    public string LastUpdatedBy { get; set; }

	    public string CreatedBy { get; set; }

        public bool IsThridPartyIdpConfigured { get; set; }
        public string Domain { get; set; }
        public bool Editable { get; set; }
        public String Comments { get; set; }
    }

    public class SupportAccountDomainModel
    {
        public string Domain { get; set; }
        public bool IsThridPartyIdpConfigured { get; set; }
        public bool Editable { get; set; }
    }
}