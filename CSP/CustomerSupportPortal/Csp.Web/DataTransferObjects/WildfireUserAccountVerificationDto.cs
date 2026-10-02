using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Attributes;

namespace Csp.Web.DataTransferObjects {

	public class WildfireUserAccountVerificationDto : DefaultDto {

		#region Public Properties

		[StringLength(200),
		Required(ErrorMessage = "Email is required"),
		Email(ErrorMessage = "Please enter a valid email.")]
		public string Email { get; set; }

		#endregion

	}
}