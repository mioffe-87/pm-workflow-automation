using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Security;

namespace Csp.Api.Model {

	/// <summary>
	/// Group of privileges that can be assigned. 
	/// The Role is designed to hold a list of Privilege objects that define granular access to the system. 
	/// The Role is made visible to the user for easy management of security. 
	///    Roles are stored in the AccountType, SupportAccount, Group and Membership.					
	/// </summary>
	public class Role : AuditCspModel {

		#region Constructors

		public Role() {

			Privileges = new List<Privilege>();
		    IsActive = true;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess(RoleFields.RoleId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			get;
			set;
		}


		/// <summary>
		/// Role name.
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		Required(ErrorMessage = "Role name is required")]
		[DataAccess(RoleFields.Name)]
		public string Name {
			get;
			set;
		}


		/// <summary>
		/// Role description.
		/// </summary>
		[DataAccess(RoleFields.Description)]
		public string Description {
			get;
			set;
		}


		/// <summary>
		/// SecurityClassification is an Enum used to join permissions with a set of data.
		/// </summary>
		[DataAccess(RoleFields.SecurityClassification, DataType = DbType.Int32)]
		public SecurityClassification Classification {
			get;
			set;
		}


		/// <summary>
		/// Link between the role and privilege.					
		/// </summary>
		public List<Privilege> Privileges {
			get;
			set;
		}


		/// <summary>
		/// Category is an Enum.
		/// </summary>
		[DataAccess(RoleFields.Category, DataType = DbType.Int32)]
		public RoleCategory Category {
			get;
			set;
		}

        /// <summary>
        /// Role is expired or not - ThreatResearcher and Autofocus Trials roles only
        /// </summary>
        public bool IsActive { get; set; }

        public bool IsDisabled { get; set; }

		#endregion

	}
}
