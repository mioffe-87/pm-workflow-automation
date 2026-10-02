using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class CsspPreLicenseDeviceDto : DefaultDto
    {
        public long SupportAccountId { get; set; }
        public List<CsspPreLicenseDevice> Devices { get; set; }
        public string DefaultDeviceCode { get; set; }
        public string DefaultDeviceSearchField { get; set; }
    }
}