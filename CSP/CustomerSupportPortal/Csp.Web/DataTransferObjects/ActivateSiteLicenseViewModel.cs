using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class ActivateSiteLicenseViewModel : DefaultDto
    {
        [Required(ErrorMessage = "Authorization code is required."),
         StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
         Remote("IsSiteLicenseAuthorizationCodeValid", "Validation")]
        public string AuthCode { get; set; }

        public EulaModel Eula { get; set; }

        public string ApiKey { get; set; }
    }
}