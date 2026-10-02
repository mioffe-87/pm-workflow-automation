using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Web.DataTransferObjects {

	public class MembershipListDto : DefaultDto {

		#region "Constuctor"

		public MembershipListDto() {
			Memberships = new List<SupportAccountMembershipModel>();
		}

		#endregion


		#region Public Properties

		public int MembershipTypeId { get; set; }

		public long MembershipParentId { get; set; }

		public List<NameValue> AvailableRoles { get; set; }
		public List<NameValue> DefaultRoles { get; set; }

        public long NumberOfPendingMemberships { get; set; }

		public List<SupportAccountMembershipModel> Memberships {
			get;
			set;
		}

        public bool IsMfaRequired { get; set; }
        
        public bool IsUserImpersonated { get; set; }
        public bool IsUserInternal { get; set; }

        public bool AllowMfaEdit { get; set; }
        #endregion

    }
}