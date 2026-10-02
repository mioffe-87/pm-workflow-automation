using Csp.Api.Model.LicenseManagement;
using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ILicenseManagementLogic
    {
        IEnumerable<LicenseManagementActivatedListItem> GetActivatedLicenses(long supportAccountId);
        LicenseManagementAvailableData GetAvailableLicenses(long supportAccountId);
        LicenseDeactivateResponse DeactivateLicenses(LicenseManagementDeactivationRequest iotDeactivationRequest);
        PanServiceRegenerateMagicLinkResponse GetActivationLink(List<string> authCodes, long supportAccountId);
    }
}