using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Services.Contracts;
using FluentValidation;
using MySql.Data.MySqlClient;

namespace Csp.Api.Model {

	/// <summary>
	/// Company table holds detailed information about support account which has PANW device(s). 
	/// The SupportAccount represents the support account for any entity that accesses the CSP. 
	/// The SupportAccount has an association and owns Devices, Entitlements, Groups and addresses.					
	/// </summary>
	public class SupportAccount : AuditCspModel {
		public SupportAccount() {


			Address = new Address();
			DefaultRoles = new List<Role>();

		}

		/// <summary>
		/// Company id.
		/// </summary>
		[DataAccess(SupportAccountFields.SupportAccountId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// SAP id from SAP
		/// </summary>
		[DataAccess(SupportAccountFields.SAPId, DataType = DbType.Int32)]
		public long SAPMasterId {
			set;
			get;
		}


		/// <summary>
		/// Updated by sync application, used as a lookup of for a new user to join to the existing account.
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		DataAccess(SupportAccountFields.SalesforceAccountId)]
		public string SalesforceId {
			set;
			get;
		}


		/// <summary>
		/// The type of account and roles that are to be associated with the SupportAccount. In CSP schema Foreign key Account type table.
		/// </summary>
		[DataAccess(SupportAccountFields.AccountTypeId, DataType = DbType.Int64)]
		public long AccountTypeId {
			set;
			get;
		}

		/// <summary>
		/// Used to determine if the support account is a support center (ASC)
		/// </summary>
		[DataAccess(MembershipFields.IsJiveSupportCenterMember, DataType = DbType.Boolean)]
		public bool IsJiveSupportCenterMember {
			set;
			get;
		}


		/// <summary>
		/// Used to determine if the support account is a training center (TSC)
		/// </summary>
		[DataAccess(MembershipFields.IsJiveTrainingCenterMember, DataType = DbType.Boolean)]
		public bool IsJiveTrainingCenterMember {
			set;
			get;
		}


		/// <summary>
		/// Used to determine if the support account has direct customer support
		/// </summary>
		[DataAccess(MembershipFields.HasJiveDirectCustomerSupport, DataType = DbType.Boolean)]
		public bool HasJiveDirectCustomerSupport {
			set;
			get;
		}

		// A convenience method that returns the AccountType object via the AccountTypeId.  The method
		// uses a cached dictionary for immediate response time.
		public AccountType AccountType {
			get { return (DependencyResolver.Current.GetService<ISupportAccountLogic>()).GetAccountType(AccountTypeId); }
		}

		public string AccountTypeName {
			get { return (AccountType != null) ? AccountType.Name : ""; }
		}

		/// <summary>
		/// Company name.
		/// </summary>
		private string _name;
        [StringLength(250, ErrorMessage = "Maximum length is 250 characters"),
		 Required(ErrorMessage = "Company name is required")]
        [AllowHtml]
		public string Name {
            // Comments By LK On 03/02 
            // Remove the custom encoding/decoding by applying the AllowHtml attribute to Name property on SupportAccount model
            //set { _name = ApiCommon.DecodeString(value); }
			set { _name = value; }
			get { return _name; }
		}

		[DataAccess(SupportAccountFields.Name)]
		public string NameEncoded {
			set { Name = value; }
            // Comments By LK On 03/02 
            // Remove the custom encoding/decoding by applying the AllowHtml attribute to Name property on SupportAccount model
            //get { return ApiCommon.EncodeString(Name); }
			get { return Name; }
		}

		/// <summary>
		/// 
		/// </summary>
		[DataAccessModel]
		public Address Address {
			set;
			get;
		}


		/// <summary>
		/// 
		/// </summary>
		[StringLength(40, ErrorMessage = "Maximum length is 40 characters"),
		DataAccess(SupportAccountFields.PhoneNumber)]
		public string PhoneNumber {
			set;
			get;
		}


		/// <summary>
		/// 
		/// </summary>
		[DataAccess(SupportAccountFields.IsActive, DataType = DbType.Boolean)]
		public bool IsActive {
			set;
			get;
		}


		/// <summary>
		/// Opt out from email notifications.
		/// </summary>
		[DataAccess(SupportAccountFields.isOptOutOfAllEmails, DataType = DbType.Boolean)]
		public bool IsOptOutOfAllEmails {
			set;
			get;
		}


		/// <summary>
		/// 
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		DataAccess(SupportAccountFields.CustomerNumber)]
		public string CustomerNumber {
			set;
			get;
		}

		[DataAccess(SupportAccountFields.SupportAccountLogo, DataType = DbType.Binary)]
		public System.Byte[] SupportAccountLogo {
			set;
			get;
		}

		[DataAccess(SupportAccountFields.LogoContentType)]
		public string LogoContentType {
			set;
			get;
		}

		/// <summary>
		///Determines the initial list of roles automatically assigned to a member when added.					
		/// </summary>
		public List<Role> DefaultRoles {
			set;
			get;
		}

        [DataAccess(SupportAccountFields.IsMfaEnabled, DataType = DbType.Boolean)]
        public bool IsMfaEnabled { get; set; }

        [DataAccess(SupportAccountFields.CanImpersonate, DataType = DbType.Int64)]
        public long ImpersonateSetting { get; set; }

        public bool IsFedramp { get; set; }

		public bool IsPilotCustomer { get; set; }

        public bool isAIEligible { get; set; }

		public string ImpersonateSettingAsString { get; set; }

		public bool IsClassicEligible { get; set; }

        public bool ShowHomepageBanner { get; set; }

    }
}
