using System.Collections.Generic;
using Csp.Api.Model;
namespace Csp.Web.DataTransferObjects {

	public class SapSupportAccountDto : DefaultDto {

		#region Public Properties

		public long SupportAccountId {
			get;
			set;
		}

		public List<NameValue> Accounts {
			get;
			set;
		}

		#endregion
	}
}