using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Security;

namespace Csp.Api.Model {

	/// <summary>
	/// The permission that determine access the functions of the system. 
	/// The Privilege is used to present granular access to the user. 
	/// Different Privileges are grouped in a Role. 
	/// The Privilege holds the description that lets a user of the 
	/// system know what each type of permission is and how it is applied.					
	/// </summary>
	public class Privilege : AuditCspModel {

		#region Public Properties

		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess(PrivilegeFields.PrivilegeId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			get;
			set;
		}


		/// <summary>
		/// Privilege name.
		/// </summary>
		[DataAccess(PrivilegeFields.Name)]
		public string Name {
			get;
			set;
		}


		/// <summary>
		/// Privilege description.
		/// </summary>
		[DataAccess(PrivilegeFields.Description)]
		public string Description {
			get;
			set;
		}


		/// <summary>
		/// Permission level specified in Enum to allow strongly typed permissions.
		/// </summary>
		[DataAccess(PrivilegeFields.Permission, DataType = DbType.Int32)]
		public Permission Permission {
			get;
			set;
		}

		#endregion

	}
}
