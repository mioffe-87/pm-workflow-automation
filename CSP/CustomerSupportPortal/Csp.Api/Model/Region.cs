using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

	/// <summary>
	/// List of State, Provinces, etc.					
	/// </summary>
	public class Region : AuditCspModel {

		#region Public Properties

		/// <summary>
		/// Primary key
		/// </summary>
		[DataAccess(RegionFields.RegionId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}

		/// <summary>
		/// Country Id
		/// </summary>
		[DataAccess(RegionFields.CountryId, DataType = DbType.Int64)]
		public long CountryId {
			set;
			get;
		}

		/// <summary>
		/// Region code
		/// </summary>
		[DataAccess(RegionFields.Code)]
		public string Code {
			set;
			get;
		}


		/// <summary>
		/// Region name
		/// </summary>
		[DataAccess(RegionFields.Name)]
		public string Name {
			set;
			get;
		}

		#endregion

	}
}
