using Csp.Api.Model.LicenseManagement;
using System.Collections.Generic;

namespace Csp.Api.Services.Contracts
{
    public interface ILicenseManagementDataAccess
    {
        IEnumerable<LicenseManagementActivatedListItem> GetActivatedLicenses(long supportAccountId);
        LicenseManagementAvailableData GetAvailableLicenses(long supportAccountId);
    }
}