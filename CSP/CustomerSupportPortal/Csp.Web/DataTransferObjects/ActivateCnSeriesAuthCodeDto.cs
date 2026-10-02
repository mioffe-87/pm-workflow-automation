using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class ActivateCnSeriesAuthCodeDto : DefaultDto
    {
        [Required]
        public string AuthorizationCode { get; set; }

        public EulaModel Eula { get; set; }

        public List<string> History { get; set; }

        public ActivateCnSeriesAuthCodeDto()
        {
            History = new List<string>();
        }
    }
}