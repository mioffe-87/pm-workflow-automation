using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class ProductModelListDto : DefaultDto {
		#region "Constuctor"

		public ProductModelListDto() {
			ProductModels = new List<ProductModelListItem>();
		}

		#endregion

		#region Public Properties

		public List<ProductModelListItem> ProductModels {
			get;
			set;
		}

		#endregion

	}
}