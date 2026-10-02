using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class EpViewModelDto : DefaultDto {
        public IEnumerable<AvailableEp> AvailableEps { get; set; }
				public List<DeviceCategory> AllDeviceCategories { get; set; }
				public string DefaultDeviceCode { get; set; }
    }
}