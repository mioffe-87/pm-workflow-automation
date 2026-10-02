using System.Collections.Generic;
using Csp.Api.Services.Contracts;

namespace Csp.Web.DataTransferObjects {

	public class WildfireUserAccountListDto : DefaultDto {

		#region "Constuctor"

		#endregion


		#region Public Properties

		public List<WildfireUserAccountItem> WildfireUserAccounts {
			get;
			set;
		}

		#endregion

	}
}