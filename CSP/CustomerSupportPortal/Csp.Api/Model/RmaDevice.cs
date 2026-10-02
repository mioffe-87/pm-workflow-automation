using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model
{
    public class RmaDevice
    {
        public long DeviceId { get; set; }

        public string SerialNumber { get; set; }

        public string ModelName { get; set; }

        public string SfdcAccountId { get; set; }

        public string SfdcContactId { get; set; }

        public string DistributorId { get; set; }

        public string EndCustomerId { get; set; }

        public string ShippingAddress { get; set; }

        public string ShippingCity { get; set; }

        public string ShippingRegion { get; set; }

        public string ShippingCountry { get; set; }

        public string ShippingZipCode { get; set; }

        public string TechSupportFile { get; set; }

        public DateTime UploadedOn { get; set; }

        public string Status { get; set; }

        public string StatusMessage { get; set; }

        public string Verdict { get; set; }
    }
}
