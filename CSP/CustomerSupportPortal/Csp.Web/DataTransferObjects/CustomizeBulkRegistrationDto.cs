using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class CustomizeBulkRegistrationDto : DefaultDto
    {
        public long? BulkRegistrationId { get; set; }
        public long SupportAccountId { get; set; }
        public bool IsReviewMode { get; set; }

        public IEnumerable<CountryDto> Countries { get; set; }
        public IEnumerable<RegionDto> Regions { get; set; }
        public IEnumerable<NameValue> DeviceCategories { get; set; }
        public IEnumerable<BulkRegistrationDeviceModel> BulkRegistrationDevices { get; set; }
        public EulaModel Eula { get; set; }
    }
}