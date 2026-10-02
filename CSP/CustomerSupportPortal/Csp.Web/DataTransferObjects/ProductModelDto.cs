using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class ProductModelDto : DefaultDto {
		public ProductModel ProductModel {
			get;
			set;
		}

		public string ProductModelName {
			get;
			set;
		}
	}
}