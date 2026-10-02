using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.Xsoar;

namespace Csp.Api.Services.Contracts
{
    public interface ISfdcEntitlementLogic
    {
        IEnumerable<SfdcEntitlement> GetEntitlementsBySupportAccountId(SfdcEntitlementRequest sfdcEntitlementRequest, long supportAccountId = 0);
        IEnumerable<AvailableSaaSActivations> AddXdrEntitlements(IEnumerable<AvailableSaaSActivations> entitlements, long[] supportAccountIds, List<string> ascSalesforceAccountId = null, bool isAscSearchOnly = false);
        IEnumerable<SfdcEntitlement> GetXSOAREntitlements(SfdcEntitlementSearchCriteria criteria, string status);
        SfdcEntitlementResponse GetSfdcEntitlements(SfdcEntitlementRequest sfdcEntitlementRequest, long[] supportAccountId, List<string> ascSalesforceAccountIds = null, bool isAscSearchOnly = false);
        IEnumerable<AvailableSaaSActivations> AddExpanseEntitlements(IEnumerable<AvailableSaaSActivations> entitlements, long[] supportAccountIds, List<string> ascSalesforceAccountId = null, bool isAscSearchOnly = false);

    }
}