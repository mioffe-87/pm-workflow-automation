using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.Data;
using Csp.Api.Model.Enums;
using LINQtoCSV;
using Csp.Api.HelperClasses;

namespace Csp.Api.Model {

	public class MembershipListItem : AuditCspModel {

		[DataAccess(MembershipListItemFields.MembershipId, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
		public long MembershipId {
			get;
			set;
		}

		[DataAccess(MembershipListItemFields.UserAccountId, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
		public long UserAccountId {
			get;
			set;
		}

		[DataAccess(MembershipListItemFields.FirstName)]
		public string FirstName {
			get;
			set;
		}


		[DataAccess(MembershipListItemFields.LastName)]
		public string LastName {
			get;
			set;
		}


		[DataAccess(MembershipListItemFields.DisplayName)]
		public string DisplayName {
			get;
			set;
		}


		[StringLength(200),
		 Required(ErrorMessage = "Email is required for memberships"),
		 Email(ErrorMessage = "Please enter a valid membership email."),
		 Remote("IsValidNonPublicEmail", "Validation"),
		 DataAccess(MembershipListItemFields.Email)]
		public string Email {
			get {
				if (_email != null) {
					return _email.Trim();
				}
				return _email;
			}
			set { _email = value; }
		}
		private string _email;

		[IsDateAfterStart("ActivationDate", ErrorMessage = "Date must be later than activation"),
		DataAccess(MembershipListItemFields.ExpirationDate, DataType = DbType.DateTime)]
		public DateTime? ExpirationDate {
			get;
			set;
		}

		[Required(ErrorMessage = "Activation Date is Required"),
		DataAccess(MembershipListItemFields.ActivationDate, DataType = DbType.DateTime)]
		public DateTime ActivationDate {
			get;
			set;
		}

		[StringLength(255, ErrorMessage = "Maximum length is 255 characters"),
			DataAccess(MembershipFields.Description)]
		public String Description {
			set;
			get;
		}

		[StringLength(128, ErrorMessage = "Maximum length is 128 characters"),
			DataAccess(MembershipFields.Comments)]
		public String Comments {
			set;
			get;
		}


		[DataAccess(MembershipListItemFields.IsSuperUser, DataType = DbType.Int64)]
		public long IsSuperUser { get; set; }

		public bool IsSuperUserBool {
			get { return IsSuperUser == 1; }
		}

		[DataAccess(MembershipListItemFields.RoleNames)]
		public string RoleNames { get; set; }

		private List<NameValue> _roles;
        public List<NameValue> Roles
        {
            get
            {
                if (!string.IsNullOrEmpty(RoleNames))
                {
                    var roles = RoleNames.Split(',').Select(r =>
                                                            new NameValue()
                                                            {
                                                                Name = r.Split('|')[0].Equals(Model.RoleNames.GroupAutofocusTrial)
                                                                        ? string.Format("{0} (Expiration Date - {1:d})", Model.RoleNames.GroupAutofocusTrial, AutoFocusTrialExpirationDate)
                                                                        : r.Split('|')[0],
                                                                Value = r.Split('|')[1],
                                                                IsActive = r.Split('|')[0].Contains(Model.RoleNames.ThreatResearcher) ? HasActiveAutofocusLicense :
                                                                           r.Split('|')[0].Equals(Model.RoleNames.GroupAutofocusTrial) ? AutoFocusTrialUser : true
                                                            })
                                                             .ToList();

                    return roles;
                }

                return _roles;
            }
            set { _roles = value; }
        }

		public int MembershipTypeId {
			get;
			set;
		}

		public long MembershipParentId {
			get;
			set;
		}

        [DataAccess(MembershipListItemFields.IsMfaEnabled, DataType = DbType.Boolean)]
        public bool IsMfaEnabled { get; set; }

	    [DataAccess(MembershipListItemFields.MultiFactorAuthType, DataType = DbType.Int16)]
	    public MultiFactorAuthType MultiFactorAuthType { get; set; }

        public bool IsMfaRequired { get; set; }

        public string MfaStatus
        {
            get { return MultiFactorAuthType.ToDescription(); }
        }

        [DataAccess(MembershipListItemFields.EnableEmailMfa, DataType = DbType.Boolean)]
        public bool EnableEmailMfa { get; set; }

        #region AutoFocus related properties

        private bool _hasActiveAutofocusLicense;
        public bool HasActiveAutofocusLicense {
            get
            {
                if( CountOfActiveAutofocusLicense > 0 ) return true ;
                return _hasActiveAutofocusLicense;
            }
            set { _hasActiveAutofocusLicense = value; }
        }

        public bool AutoFocusTrialUser { get; set; }

        [DataAccess(MembershipListItemFields.AutoFocusTrialExpirationDate, DataType = DbType.DateTime)]
        public DateTime? AutoFocusTrialExpirationDate { get; set; }

        public string Notification { get; set; }

        #endregion

        public bool Editable { get; set; }

        public string PreviousRoles { get; set; }

        public bool IsThridPartyIdpConfigured { get; set; }
        public string Domain { get; set; }

        [DataAccess(MembershipListItemFields.CountOfActiveAutofocusLicense, DataType = DbType.Int64)]
        public long CountOfActiveAutofocusLicense { get; set; }
    }

	public class MembershipListItemCsv {
		[CsvColumn(FieldIndex = 1, Name = "Name")]
		public string Name { get; set; }

		[CsvColumn(FieldIndex = 2, Name = "Email")]
		public string Email { get; set; }

		[CsvColumn(FieldIndex = 3, Name = "Activation Date")]
		public string ActivationDate { get; set; }

		[CsvColumn(FieldIndex = 4, Name = "Expiration Date")]
		public string ExpirationDate { get; set; }

	    [CsvColumn(FieldIndex = 5, Name = "Description")]
	    public string Description { get; set; }

	    [CsvColumn(FieldIndex = 6, Name = "Previous Roles")]
	    public string PreviousRoles { get; set; }

        [CsvColumn(FieldIndex = 7, Name = "Current Roles")]
		public string Roles { get; set; }

	    [CsvColumn(FieldIndex = 8, Name = "Edited by ")]
	    public string LastUpdatedBy { get; set; }

        //[CsvColumn(FieldIndex = 9, Name = "2 Factor Authentication")]
        //public string MfaStatus { get; set; }

	    [CsvColumn(FieldIndex = 9, Name = "Created by ")]
	    public string CreatedBy { get; set; }

		[CsvColumn(FieldIndex = 10, Name = "Comments")]
	    public string Comments { get; set; }
       
	}

}
