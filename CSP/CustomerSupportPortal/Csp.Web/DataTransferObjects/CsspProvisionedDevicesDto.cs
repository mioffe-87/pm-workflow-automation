using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class CsspProvisionedDevicesDto : DefaultDto
    {
        public bool HasCsspAccess { get; set; }
        public string DefaultDeviceCode { get; set; }
        public long SupportAccountId { get; set; }
        public List<CsspProvisionedDevices> Devices { get; set; }
        public List<KeyValuePair<string, string>> DownLoadUsageReportMonths { get; set; }
    }
}