using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

	/// <summary>
	/// Holds detail device part type information. Reference table for partmaster.					
	/// </summary>
	public class PartType : AuditCspModel {

		#region Public Properties

		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess(PartTypeFields.PartTypeId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// Type Description.
		/// </summary>
		[DataAccess(PartTypeFields.Description)]
		public string Description {
			set;
			get;
		}

		#endregion

	}
}
