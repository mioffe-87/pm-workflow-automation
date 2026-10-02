using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model;
using Country = Csp.Api.Model.Country;

namespace Csp.Web.DataTransferObjects {

	public class WildfireUserAccountDto : DefaultDto {

		#region Public Properties

		public WildfireUserAccount UserAccount { get; set; }

		[Required(ErrorMessage = "Region/State is required")]
		public string OtherRegion { get; set; }

		public List<Country> Countries { get; set; }

		#endregion

	}
}