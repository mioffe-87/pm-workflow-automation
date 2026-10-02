using Csp.Api.Model;
using Csp.Api.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IZtpLogic
    {
        IEnumerable<PanoramaAssociationResult> GetPanoramaAssociations(long supportAccountId);

        bool AddOrUpdatePanoramaAssociation(PanoramaAssociationRequest requestData);

        List<NameValueOnly> GetDevicesForZtp(long supportAccountId);

        IEnumerable<PanoramaAssociationHistory> GetPanoramaAssociationHistory(long supportAccountId);

        bool IsPanormaDeviceActive(long supportAccountId, string serialNumber);
    }
}
