using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace Csp.Web.DataTransferObjects {

	public class ActivateLicense {
		public bool IsReplacementBundle {
			get {

				return (LicenseBundleModel != String.Empty) && (DeviceBundleModel != String.Empty) && (LicenseBundleModel != DeviceBundleModel);
			}
		}


		public bool IsSiteLicense {
			get;
			set;
		}

		public bool Is4HourRma {
			get;
			set;
		}

		public string SerialNumber {
			get;
			set;
		}

		public string LicenseBundleModel {
			get;
			set;
		}

		public string DeviceBundleModel {
			get;
			set;
		}

		[Required(ErrorMessage = "Authorization code is required."),
		 StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		 Remote("IsAuthorizationCodeValid", "Validation", AdditionalFields = "SerialNumber")]
		public string AuthorizationCode {
			get;
			set;
		}


		public string City {
			get;
			set;
		}

		public string PostalCode {
			get;
			set;
		}

		public bool ActivateSuccessful { get; set; }

        [Required(ErrorMessage = "Upgrade Auth code is required."),
         StringLength(45, ErrorMessage = "Maximum length is 45 characters")
         //Remote("IsAuthorizationCodeValidForUpgrade", "Validation", AdditionalFields = "SerialNumber")
        ]
        public string UpgradeAuthorizationCode
        {
            get;
            set;
        }

        public bool IsUpgradeLicense { get; set; }

        public string Country { get; set; }

        public long BillToCountryId { get; set; }

        public long CountryId { get; set; }

        public bool Show4HrLocation { get; set; }
	}
}