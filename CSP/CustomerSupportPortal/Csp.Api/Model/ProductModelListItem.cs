using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Attributes;

namespace Csp.Api.Model {
	public class ProductModelListItem : BaseCspModel {

		#region Public Properties


		[DataAccess(ProductModelListItemFields.ModelId, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
		public long ModelId {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.Name)]
		public string Name {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.ProductLine)]
		public string ProductLine {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.Platform)]
		public string Platform {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.MinimumRequiredVersion)]
		public string MinimumRequiredVersion {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.MaximumSupportedVersion)]
		public string MaximumSupportedVersion {
			get;
			set;
		}

        [DataAccess(ProductModelListItemFields.DisplayOrder, DataType = DbType.Int64)]
        public int DisplayOrder
        {
            get;
            set;
        }

        [DataAccess(ProductModelListItemFields.DisplayName)]
        public string DisplayName
        {
            get;
            set;
        }
		#endregion

	}
}