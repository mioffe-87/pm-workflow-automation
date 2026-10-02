using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

	/// <summary>
	/// Table used to determine default security settings for different types of support accounts.   
	/// The AccountType defines the type of account and roles that are to be associated with the SupportAccount. 
	/// An AccountType is defined separate from a SupportAccount and a single AccountType is assigned to a SupportAccount.					
	/// </summary>
	public class AccountType : AuditCspModel {

		#region Constructors

		public AccountType() {
			AdministrativeRole = new Role();
			AssignableRoles = new List<Role>();
			DefaultRoles = new List<Role>();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Account Id, primary key.
		/// </summary>
		[DataAccess(AccountTypeFields.AccountTypeId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// Account type name.
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		Required(ErrorMessage = "Account Type name is required")]
		[DataAccess(AccountTypeFields.Name)]
		public string Name {
			set;
			get;
		}


		/// <summary>
		/// List of Roles associated with Account
		/// </summary>
		public List<Role> DefaultRoles {
			get;
			set;
		}


		/// <summary>
		/// The administrative role for all support account specified with a specific account type. 
		/// </summary>
		[DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = AccountTypeFields.AdministrativeRoleId, UseKeyOnly = true)]
		public Role AdministrativeRole {

			get;
			set;
		}

		/// <summary>
		/// Administrative role name.
		/// </summary>
		[DataAccess(AccountTypeFields.AdministrativeRoleName)]
		public string AdministrativeRoleName {
			set;
			get;
		}


		/// <summary>
		///  List of AssignableRoles for Account
		/// </summary>
		public List<Role> AssignableRoles {
			get;
			set;
		}


		/// <summary>
		/// Determines access to different information. SecurityClassification is an Enum used to join permissions with a set of data.
		/// </summary>
		[DataAccess(AccountTypeFields.SecurityClassification, DataType = DbType.Int32)]
		public SecurityClassification Classification {
			get;
			set;
		}

		#endregion

	}
}

