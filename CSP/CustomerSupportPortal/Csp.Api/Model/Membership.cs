using System;
using System.Collections.Generic;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model {

	/// <summary>
	/// Infromation pertaining to user the user account relationship to support accont , group or device. 
	/// The Membership defines the relationship between a UserAccount and a SupportAccount, Group or Device. 
	/// The Membership contains information about the relationship between a UserAccount and the object in which they are a member. 
	/// Membership has roles assigned to it that determine what actions the user can take for that specific membership.					
	/// </summary>
	public class Membership : AuditCspModel {

		#region Constructors

		public Membership() {

			Roles = new List<Role>();
			UserAccount = new UserAccount();

		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess(MembershipFields.MembershipId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// Each membership can have only one of value (SupportAccountId/ GroupId/DeviceId).
		/// </summary>
		[DataAccess(MembershipFields.SupportAccountId, DataType = DbType.Int64)]
		public long SupportAccountId {
			set;
			get;
		}


		/// <summary>
		/// Each membership can have only one of value (SupportAccountId/ GroupId/DeviceId).
		/// </summary>
		[DataAccess(MembershipFields.GroupId, DataType = DbType.Int64)]
		public long GroupId {
			set;
			get;
		}


		/// <summary>
		/// Each membership can have only one of value (SupportAccountId/ GroupId/DeviceId).
		/// </summary>
		[DataAccess(MembershipFields.DeviceId, DataType = DbType.Int64)]
		public long DeviceId {
			set;
			get;
		}


		/// <summary>
		/// Membership description.
		/// </summary>
		[StringLength(255, ErrorMessage = "Maximum length is 255 characters"),
		DataAccess(MembershipFields.Description)]
		public String Description {
			set;
			get;
		}


		/// <summary>
		/// Start date that the membership is valid.
		/// </summary>
		[Required(ErrorMessage = "Activation Date is Required"),
		DataAccess(MembershipFields.ActivationDate, DataType = DbType.DateTime)]
		public DateTime ActivationDate {
			set;
			get;
		}


		/// <summary>
		/// End date that the membership is valid.
		/// </summary>
		[IsDateAfterStart("ActivationDate", ErrorMessage = "Date must be later than activation"),
		DataAccess(MembershipFields.ExpirationDate, DataType = DbType.DateTime)]
		public DateTime? ExpirationDate {
			set;
			get;
		}



		/// <summary>
		/// Map to Accounts table**
		/// </summary>
		[StringLength(50, ErrorMessage = "Maximum length is 50 characters"),
		 DataAccess(MembershipFields.JiveId, DataType = DbType.String)]
		public string JiveId {
			set;
			get;
		}


		/// <summary>
		/// Map to Accounts table**
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		DataAccess(MembershipFields.SalesforceContactId, DataType = DbType.String)]
		public string SalesforceContactId {
			set;
			get;
		}


		/// <summary>
		/// Map to Accounts table**
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		DataAccess(MembershipFields.SalesforceLogInId, DataType = DbType.String)]
		public string SalesforceLogInId {
			set;
			get;
		}

		[DataAccess(MembershipFields.MembershipStatus, DataType = DbType.Int16)]
		public long MembershipStatus {
			set;
			get;
		}

		[DataAccess(MembershipFields.MembershipResponseGuid)]
		public string MembershipResponseGuid {
			set;
			get;
		}

        [DataAccess(MembershipFields.IsMfaEnabled, DataType = DbType.Boolean)]
        public bool IsMfaEnabled
        {
            set;
            get;
        }

	    public bool EnableEmailMfa { get; set; }

        public MultiFactorAuthType MultiFactorAuthType { get; set; }

        /// <summary>
        /// The user to whom the membership is created for. Foreign key to UserAccont table.
        /// </summary>
        [DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = MembershipFields.UserAccountId, UseKeyOnly = true)]
		public UserAccount UserAccount {
			set;
			get;
		}


		/// <summary>
		/// These are the role that belong to the membership for a specific support account, group or a device.					
		/// </summary>
		public List<Role> Roles {
			set;
			get;
		}

		public bool ShowThreatResearcherUseCountWarning { get; set; }
        public bool IsValidateThreatResearcherUseCount { get; set; }
        public string ThreatResearcherAssignmentError { get; set; }
        public string AutofocusTrialAssignmentError { get; set; }
        public string AutofocusTrialRoleWarning { get; set; }
        public bool ShowATCManagerUseCountWarning { get; set; }

        [DataAccess(MembershipFields.RegisteredAppId, DataType = DbType.Int64)]
        public long RegisteredAppId { get; set; }
        #endregion

    }
}
