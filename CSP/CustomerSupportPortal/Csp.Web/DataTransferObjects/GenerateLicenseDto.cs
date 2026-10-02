using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class GenerateLicenseDto : DefaultDto
    {
        public string LicenseKey { get; set; }

        public bool hasError { get; set; }

        public string ErrorMessage { get; set; }

        public string AuthorizationCode { get; set; }

        public string ExpirationDate { get; set; }

    }
}