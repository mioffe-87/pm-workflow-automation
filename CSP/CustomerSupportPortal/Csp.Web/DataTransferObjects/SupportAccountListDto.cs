using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
namespace Csp.Web.DataTransferObjects {

	public class SupportAccountListDto : DefaultDto {

		#region "Constuctor"

		public SupportAccountListDto() {
			Accounts = new List<SupportAccountListItem>();
		}

		#endregion

		#region Public Properties

		public List<SupportAccountListItem> Accounts {
			get;
			set;
		}

		#endregion
	}
}