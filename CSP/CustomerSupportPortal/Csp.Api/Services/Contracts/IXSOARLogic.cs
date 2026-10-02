using Csp.Api.Model;
using Csp.Api.Model.Xsoar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IXSOARLogic
    {
        IEnumerable<XSOARListItem> GetXSOARProductDetails(long supportAccountId, string status);
        string GetXSOARLicenseData(string serialNumber, string status);
        IEnumerable<XsoarMarketplacePurchaseDataModel> GetMarketplaceDetails(long supportAccountId);
    }
}