using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

	/// <summary>
	/// Master table for device and entitlement parts information.					
	/// </summary>
	public class PartMaster : AuditCspModel {
		public PartMaster() {

			Type = new PartType();
			SubType = new PartSubType();

		}

		/// <summary>
		/// Device/Entitlement part number..
		/// </summary>
		[DataAccess(PartMasterFields.PartNumber)]
		public string PartNumber {
			set;
			get;
		}


		/// <summary>
		/// Template used to create license key by licensing service.
		/// </summary>
		[DataAccess(PartMasterFields.LicenseTemplate)]
		public string LicenseTemplate {
			set;
			get;
		}


		/// <summary>
		/// Partmaster id, primary Key
		/// </summary>
		[DataAccess(PartMasterFields.PartMasterId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// Type of the device/entitlement, refer part_type table. In CSP schema foreign key to PartType table.
		/// </summary>
		[DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = PartMasterFields.PartTypeId, UseKeyOnly = true)]
		public PartType Type {
			set;
			get;
		}


		/// <summary>
		/// Description detail.
		/// </summary>
		[DataAccess(PartMasterFields.Description)]
		public string Description {
			set;
			get;
		}


		/// <summary>
		/// Model compatible with this part number.
		/// </summary>
		[DataAccess(PartMasterFields.ModelCompatibility)]
		public string ModelCompatibility {
			set;
			get;
		}


		/// <summary>
		/// Type of the device/entitlement, refer part_type table. In CSP schema foreign key to PartSubType table.
		/// </summary>
		[DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = PartMasterFields.PartSubtypeId, UseKeyOnly = true)]
		public PartSubType SubType {
			set;
			get;
		}


		/// <summary>
		/// How many time the key is used. If 0 then used one time only.
		/// </summary>
		[DataAccess(PartMasterFields.UseCount, DataType = DbType.Int64)]
		public long UseCount {
			set;
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		public string Id1 {
			set;
			get;
		}
	}
}
