using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class ActivateSaasAuthCodeDto : DefaultDto
    {
        public long SupportAccountID { get; set; }
        [Required]
        public string AuthorizationCode { get; set; }

        public string Panorama { get; set; }

        public List<string> Panoramas { get; set; }

        public string LoggingServiceDevice { get; set; }

        public string LoggingRegion { get; set; }

        public EulaModel Eula { get; set; }

        public bool IsPanoramaAssociation { get; set; }

        public bool IsLogQuotaChecked { get; set; }
    }


}