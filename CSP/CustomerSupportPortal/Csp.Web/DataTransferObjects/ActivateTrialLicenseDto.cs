using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Csp.Web.DataTransferObjects {

	public class ActivateTrialLicenseDto : DefaultDto {

		#region public members

		public List<object> AvailableLicenses;

		public string SerialNumber {
			get;
			set;
		}

		public string Mode {
			get;
			set;
		}

		public bool IsEligibleForDpmLicense { get; set; }

        public bool IsEligibleForDpbLicense { get; set;}

        public bool IsEligibleForPanoramaLicense { get; set; }

        public RegisterVmDto VMRegistrationDto { get; set; }
		
        public bool IsPostback { get; set; }

		#endregion
	}
}