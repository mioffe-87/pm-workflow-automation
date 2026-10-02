using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;

namespace Csp.Web.DataTransferObjects {

	public class SupportAccountDto : DefaultDto {

		#region Private Members

		private SupportAccount supportAccount;

		#endregion


		#region Public Properties

		public SupportAccountDto() {

			RoleAssignment = new RoleAssignmentDto();
		}

		public string AccountType {
			get;
			set;
		}

		public SupportAccount SupportAccount {
			get {
				return supportAccount;
			}
			set {
				supportAccount = value;
			}
		}

		public RoleAssignmentDto RoleAssignment {
			get;
			private set;
		}

		[File(FileSize = 100 * 1024 * 1024, FileTypes = new[] { ".gif", ".jpg", ".jpeg", ".png" })]
		public HttpPostedFileBase CompanyLogo { get; set; }

		[Remote("AccountConversionDeviceCodeIsValid", "Validation", AdditionalFields = "ConvertAccount")]
		public string AccountConversionDeviceCode { get; set; }

		[Remote("AccountConversionCustomerCodeIsValid", "Validation", AdditionalFields = "ConvertAccount")]
		public string AccountConversionCustomerCode { get; set; }

        public bool ConvertAccount { get; set; }

        public RegistrationCodeStatus RegistrationCodeStatus { get; set; }

        public bool RegistrationRequiresSuApproval { get; set; }

        public bool EnableMfa { get; set; }

		#endregion


		public List<string> CurrentRoles { get; set; }
		public bool IsThirdPartyIdpEnabled { get; set; }
		public string ThirdPartyIdpEntityId { get; set; }
		public bool IsDomainAdministrator { get; set; }
        public bool IsFedRampAccount { get; set; }
        public bool IsMSSPAccount { get; set; }
        public string ThirdPartySsoConfigUrl { get; set; }
        public ImpersonateStatus ImpersonateSetting { get; set; }
        public bool ShowThirdPartyIdpSettings { get; set; }
    }
}