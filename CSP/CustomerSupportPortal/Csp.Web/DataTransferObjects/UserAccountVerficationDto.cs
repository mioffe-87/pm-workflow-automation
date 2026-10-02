using System.Web.Mvc;
using Csp.Api.Model.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Csp.Web.DataTransferObjects {

	public class UserAccountVerificationDto : DefaultDto {
	    [Required(ErrorMessage = "Email is required"),
		Email(ErrorMessage = "Please enter a valid email."),
        //Remote("IsValidNonPublicEmail", "Validation"),
		StringLength(200, ErrorMessage = "Maximum length is 200 characters")]
		public string Email { get; set; }

        //[Required(ErrorMessage = "Captcha validation failed.Please try again")]
        ////[Display(Name = "Enter the characters from the picture")]
        //public string CaptchaCode { get; set; }

        public string SelfRegistrationCode { get; set; }
        public bool ShowPleaseUseBizAddressWarning { get; set; }
	}
}