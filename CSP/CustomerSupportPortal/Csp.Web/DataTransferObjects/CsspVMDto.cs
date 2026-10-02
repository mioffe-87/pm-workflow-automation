using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class CsspVmDto : DefaultDto
    {
        public bool HasCsspAccess { get; set; }
        public bool CanGeneratePrelicenseKeys { get; set; }
        public string DefaultDeviceCode { get; set; }
        public IEnumerable<AvailableVm> AvailableVms { get; set; }
        public long SupportAccountId { get; set; }
    }
}