using System;
using System.Web.Mvc;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model
{

    /// <summary>
    /// Accounts (User) table holds detailed information about users who have access to CSP portal. 
    /// The UserAccount represents a person that has access to the CSP. 
    /// The UserAccount may have Membership to a SupportAccount, Group or a Device.					
    /// </summary>
    public class UserAccount : AuditCspModel
    {

        #region Constructors

        public UserAccount()
        {

            Address = new Address();

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Account Id, primary key.
        /// </summary>
        [DataAccess(UserAccountFields.UserAccountId, DataType = DbType.Int64, UseForCreate = false)]
        public long Id
        {
            get;
            set;
        }


        /// <summary>
        /// User Email address.
        /// </summary>
        [StringLength(200, ErrorMessage = "Maximum length is 200 characters"),
        Required(ErrorMessage = "Email is required"),
        Email(ErrorMessage = "Please enter a valid email."),
        DataAccess(UserAccountFields.Email)]
        public string Email
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        [StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
        Required(ErrorMessage = "Display Name is required"),
        // Comments By LK
        // Only Allow Alphanumeric characters
        // RegularExpression(@"(\S)+", ErrorMessage = "White Space is not allowed in Display Name"),
        RegularExpression(@"^(?=.{6,30}$)(([a-zA-Z0-9]+)|[a-zA-Z0-9]+[\._-]{1}[a-zA-Z0-9]+)$",
            ErrorMessage = "Display Name must be between 6 and 30 characters, with alphanumeric characters or " +
                           "following special characters and without spaces . - _ (no consecutive special characters)"),
        Remote("IsDisplayNameAvailable", "Validation", AdditionalFields = "Id"),
        DataAccess(UserAccountFields.DisplayName)]
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// User Title
        /// </summary>
        [StringLength(60, ErrorMessage = "Maximum length is 60 characters"),
        DataAccess(UserAccountFields.Title)]
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// User first name.
        /// </summary>

        [StringLength(50, ErrorMessage = "Maximum length is 50 characters"),
        Required(ErrorMessage = "First Name is required"),
        DataAccess(UserAccountFields.FirstName)]
        public string FirstName
        {
            get;
            set;
        }


        /// <summary>
        /// User last name.
        /// </summary>
        [StringLength(50, ErrorMessage = "Maximum length is 50 characters"),
        Required(ErrorMessage = "Last Name is required"),
        DataAccess(UserAccountFields.LastName)]
        public string LastName
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        [DataAccessModel]
        public Address Address
        {
            get;
            set;
        }


        /// <summary>
        /// User phone.
        /// </summary>
        [StringLength(40, ErrorMessage = "Maximum length is 40 characters"),
        // Required(ErrorMessage = "Phone is required"),
        DataAccess(UserAccountFields.PhoneNumber)]
        public string PhoneNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Used for weekly email notification. User can opt in or opt out from emails which are configurable in “My contact info” screen.
        /// </summary>
        [DataAccess(UserAccountFields.IsOptInEmail, DataType = DbType.Int16)]
        public bool IsOptInEmail
        {
            get;
            set;
        }


        /// <summary>
        /// Last email sent date.
        /// </summary>
        [DataAccess(UserAccountFields.EmailLastSent, DataType = DbType.DateTime)]
        public DateTime EmailLastSent
        {
            get;
            set;
        }


        /// <summary>
        /// To check the status of the email sent out.
        /// </summary>
        [DataAccess(UserAccountFields.EmailResponse)]
        public string EmailResponse
        {
            get;
            set;
        }


        /// <summary>
        /// To send email about security related information.
        /// </summary>
        [DataAccess(UserAccountFields.IsOptInSecurityEmail, DataType = DbType.Int32)]
        public bool IsOptInSecurityEmail
        {
            get;
            set;
        }


        /// <summary>
        /// System ip address where the user is registered.
        /// </summary>
        [StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
        DataAccess(UserAccountFields.IPAddress)]
        public string IpAddress
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        [DataAccess(UserAccountFields.IsActive, DataType = DbType.Boolean)]
        public bool IsActive
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        [DataAccess(UserAccountFields.IsTempMessageViewed, DataType = DbType.Boolean)]
        public bool IsTempMessageViewed
        {
            get;
            set;
        }


        /// <summary>
        /// To send email about security related information.
        /// </summary>
        [DataAccess(UserAccountFields.IsOptInPanOsUpdateEmail, DataType = DbType.Int32)]
        public bool IsOptInPanOsUpdateEmail
        {
            get;
            set;
        }

        [DataAccess(UserAccountFields.IsMfaEnabled, DataType = DbType.Boolean)]
        public bool IsMfaEnabled
        {
            get;
            set;
        }

        [DataAccess(UserAccountFields.MultiFactorAuthType, DataType = DbType.Boolean)]
        public MultiFactorAuthType MultiFactorAuthType
        {
            get;
            set;
        }

        public bool IsGoogleAuthEnrolled { get; set; }

        [DataAccess(UserAccountFields.IsFirstLogin, DataType = DbType.Boolean)]
        public bool IsFirstLogin { get; set; }


        [DataAccess(UserAccountFields.RegisteredAppId, DataType = DbType.Int64)]
        public long RegisteredAppId { get; set; }

        [DataAccess(UserAccountFields.DefaultSupportAccountId, DataType = DbType.Int64)]
        public long? DefaultSupportAccountId { get; set; }

        public string SalesforceLoginId { get; set; }

        public bool HasMultipleMemberships { get; set; }

        public bool IsValid { get; set; }
        public string MembershipStatus { get; set; }

        public bool IsMembershipExpired { get; set; }
        public long SelectedMembershipId { get; set; }
        public string Theme { get; set; }

        #endregion

    }
}
