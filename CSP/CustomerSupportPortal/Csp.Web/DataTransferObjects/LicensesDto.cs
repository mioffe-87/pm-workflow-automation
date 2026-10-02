using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {


	public class LicensesDto : DefaultDto
	{

		public LicensesDto()
		{
			IsLicenseForCloudDevice = false;
		}

		public bool IsLicenseForCloudDevice { get; set; } // AWS, Azure

        public bool IsLicenseForCsspDevice { get; set; }

		public List<LicenseListItem> Licenses {
			get;
			set;
		}


	}
}