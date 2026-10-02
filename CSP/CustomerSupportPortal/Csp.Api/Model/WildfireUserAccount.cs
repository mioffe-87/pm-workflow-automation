using System;
using System.Web.Mvc;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.ComponentModel.DataAnnotations;
namespace Csp.Api.Model {

	public class WildfireUserAccount : AuditCspModel {

		#region Constructors

		public WildfireUserAccount() {

			Address = new Address();

		}

		#endregion

		#region Public Properties

		public long Id {
			get;
			set;
		}

		[StringLength(200),
		Required(ErrorMessage = "Email is required"),
        Email(ErrorMessage = "Please enter a valid email."),]
		public string Email {
			get;
			set;
		}

		[StringLength(25, ErrorMessage = "Maximum length is 25 characters"),
		Required(ErrorMessage = "First name is required")]
		public string FirstName {
			get;
			set;
		}

		[StringLength(50, ErrorMessage = "Maximum length is 50 characters"),
		Required(ErrorMessage = "Last name is required")]
		public string LastName {
			get;
			set;
		}

		public Address Address {
			get;
			set;
		}

		[StringLength(20, ErrorMessage = "Maximum length is 20 characters"),
		Required(ErrorMessage = "Phone is required")]
		public string PhoneNumber {
			get;
			set;
		}

		public string IpAddress {
			get;
			set;
		}

		public bool IsActive {
			get;
			set;
		}

		#endregion

	}
}
