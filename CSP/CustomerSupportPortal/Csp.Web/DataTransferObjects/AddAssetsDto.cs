using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class AddAssetsDto : DefaultDto
    {
        public long? BulkRegistrationId { get; set; }
        public string CustomerCode { get; set; }
        public string DeviceCode { get; set; }

        public IEnumerable<ClaimedAssetListItem> AvailablePurchases { get; set; }
    }
}