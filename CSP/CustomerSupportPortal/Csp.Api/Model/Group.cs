using System.Collections.Generic;
using System.Web;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Csp.Api.Model {

	/// <summary>
	/// Limits the visibility of devices, unregistered licenses and/or user accounts. 
	/// The Group exists to allow limited visibility of a Device and entitlement that belongs to a SupportAccount. 
	/// A Group has a relationship to a SupportAccount and may only belong to one SupportAccount.					
	/// </summary>
	public class Group : AuditCspModel {
		public Group() {

			Address = new Address();
			DefaultRoles = new List<Role>();
		}
		public string AdditionalFields { get; set; }

		/// <summary>
		/// Support account id to which this group is belongs to.
		/// </summary>
		[DataAccess(GroupFields.SupportAccountId, DataType = DbType.Int64)]
		public long SupportAccountId {
			get;
			set;
		}


		/// <summary>
		/// Group name.
		/// </summary>
		private string _name;
		[StringLength(50, ErrorMessage = "Maximum length is 50 characters"),
		Required(ErrorMessage = "Group name is required ")]
		public string Name {
			set { _name = ApiCommon.DecodeString(value); }
			get { return _name; }
		}

		[DataAccess(GroupFields.Name)]
		public string NameEncoded {
			set { Name = value; }
			get { return ApiCommon.EncodeString(Name); }
		}

		[DataAccess(GroupFields.GroupId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			get;
			set;
		}

		/// <summary>
		/// Group description.
		/// </summary>
		private string _description;
		[StringLength(255, ErrorMessage = "Maximum length is 255 characters")]
		public string Description {
			set { _description = ApiCommon.DecodeString(value); }
			get { return _description; }
		}

		[DataAccess(GroupFields.Description)]
		public string DescriptionEncoded {
			set { Description = value; }
			get { return ApiCommon.EncodeString(Description); }
		}

		/// <summary>
		/// Foreign key to Address table.
		/// </summary>
		[DataAccessModel()]
		public Address Address {
			get;
			set;
		}


		/// <summary>
		/// Phone number of group contact.
		/// </summary>
		[StringLength(40, ErrorMessage = "Maximum length is 40 characters"),
		DataAccess(GroupFields.PhoneNumber)]
		public string PhoneNumber {
			get;
			set;
		}

		/// <summary>
		///Determines the initial list of roles automatically assigned to a member when added.					
		/// </summary>
		public List<Role> DefaultRoles {
			set;
			get;
		}
	}
}
