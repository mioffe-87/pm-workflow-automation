using Csp.Api.Model;
using Csp.Api.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IZtpDataAccess
    {
        bool AddPanoramaAssociation(PanoramaAssociationRequest requestData, long? tenantId, string appInternalName);

        bool UpdatePanoramaAssociation(PanoramaAssociationRequest requestData, long? tenantId, string appInternalName);

        IEnumerable<PanoramaAssociationResult> GetPanoramaAssociations(long supportAccountId, string appInternalName);

        long? GetPanoramaZtpTenant(long supportAccountId, AppPortalAppType appPortalApp);

        long GenerateTenantId(int retryLimit, bool isLoggingService = false);

        List<NameValueOnly> GetDevicesForZtp(long supportAccountId);

        IEnumerable<PanoramaAssociationHistory> GetPanoramaAssociationHistory(long supportAccountId, string appInternalName);

        bool IsPanormaDeviceActive(long supportAccountId, string serialNumber);
    }
}
