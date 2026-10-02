using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model.Attributes;

namespace Csp.Web.DataTransferObjects
{
    public class VmElaGrantTokenDto : DefaultDto
    {
        public string GrandParentAuthCode { get; set; }

        [Required(ErrorMessage = "Please provide your Email address"), DisplayName("Destination Email:"), Email(ErrorMessage = "Please provide a valid Email address.")]
        public string DestinationEmail { get; set; }

        [DisplayName("Notify User:")]
        public bool NotifyUser { get; set; }
    }
}