using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Wildfire;

namespace Csp.Api.Services.Contracts
{
    public interface IWildfireDataAccess
    {
        WildfireAssetsResponse<WildfirePortalItem> GetPortals(WildfireApiRequest request);

        IEnumerable<KeyValuePair<long, string>> GetSupportAccountIdsByUserId(long userAccountId, string salesforceAccountId);

        WildfireActivationLicense GetProductTypeBasedDetails(WildfireApiActivationDetailsRequestData data);

        string GetSfdcMappedAccountId(string commercialSfdcAccountId);
    }
}