using System;
using Csp.Api.Model.Attributes;
using System.Data;

namespace Csp.Api.Model {

	/// <summary>
	/// Holds detail information like licensing feature and licensing type. Reference table for partmaster.					
	/// </summary>
	public class PartSubType : AuditCspModel {

		#region Constructors

		public PartSubType() {

			Type = new PartType();
			LicenseType = new LicenseType();

		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Sub type name.
		/// </summary>
		[DataAccess("Name")]
		public string Name {
			set;
			get;
		}


		/// <summary>
		/// Sub type description.
		/// </summary>
		[DataAccess("Description")]
		public string Description {
			set;
			get;
		}


		/// <summary>
		/// License feature categorization name type like eval, support, global protect etc.
		/// </summary>
		[DataAccess("LicenseFeaturesName")]
		public string LicenseFeatureName {
			set;
			get;
		}


		/// <summary>
		/// This column is used to identify perpetual devices.
		/// </summary>
		[DataAccess("LicenseType", DataType = DbType.Int64)]
		public LicenseType LicenseType {
			set;
			get;
		}


		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess("PartSubtypeId", DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// Type Id. In CSP schema foreign key to PartType table.
		/// </summary>
		[DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = "PartTypeId", UseKeyOnly = true)]
		public PartType Type {
			set;
			get;
		}

		#endregion

	}
}
