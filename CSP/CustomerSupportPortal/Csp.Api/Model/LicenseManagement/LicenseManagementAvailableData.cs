using Csp.Api.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Csp.Api.Model.LicenseManagement
{
    public class LicenseManagementAvailableData
    {
        public int[] LicenseTypes
        {
            get
            {
                return AvailableItems?.OrderBy(x =>  x.LicenseType)
                    .Select(x => x.LicenseType)
                    .Distinct()
                    .ToArray() ?? new int[0];
            }
        }

        public List<LicenseManagementAvailableListItem> AvailableItems { get; set; }
    }
}
