using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Web.Attributes;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Web.DataTransferObjects
{
    public class RmaDeviceDto
    {
        public bool IsSelected { get; set; }

        public bool IsReadOnly { 
            get { return (IsAnalysed || !IsSelected) ? true : false; }
        }

        public long DeviceId { get; set; }

        public string SerialNumber { get; set; }

        public string ModelName { get; set; }

        public string SfdcAccountId { get; set; }

        public string SfdcContactId { get; set; }

        public string DistributorId { get; set; }

        public string EndCustomerId { get; set; }

        public string ShippingAddress { get; set; }

        public string ShippingCity { get; set; }

        public string ShippingCountry { get; set; }

        public string ShippingRegion { get; set; }

        public string ShippingZipCode { get; set; }

        public string TechSupportFileName { get; set; }

        public DateTime UploadedOn { get; set; }

        public string Status { get; set; }

        public string StatusMessage { get; set; }

        public string Verdict { get; set; }

        public bool IsAnalysed
        {
            get { return !string.IsNullOrEmpty(Status) && !Verdict.Equals(JobVerdict.Failure.ToString()); }
        }

        public CountryDto Country { get; set; }

        public RegionDto Region { get; set; }

        public string OtherRegion { get; set; }

        public HttpPostedFileBase TechSupportFile { get; set; }
    }
}