using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class ActivateVmAuthCodeDto : DefaultDto
    {
        [Required]
        public string AuthorizationCode { get; set; }

        public EulaModel Eula { get; set; }
    }
}