using System.ComponentModel.DataAnnotations;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Attributes;

namespace Csp.Api.Model {
	/// <summary>
	/// DeviceSet table which holds the information of device groups			
	/// </summary>
	public class ProductModel : AuditCspModel {
		#region Constructors


		#endregion

		#region Public Properties


		[DataAccess(ProductModelListItemFields.ModelId, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = true)]
		public long ModelId {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.Name)]
		[Required]
		[StringLength(25)]
		public string Name {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.ProductLine)]
		[StringLength(25)]
		public string ProductLine {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.Platform)]
		[StringLength(25)]
		public string Platform {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.MinimumRequiredVersion)]
		[StringLength(15)]
		public string MinimumRequiredVersion {
			get;
			set;
		}

		[DataAccess(ProductModelListItemFields.MaximumSupportedVersion)]
		[StringLength(15)]
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