using Csp.Api.Model;
using Csp.Api.Model.Ion;
using Csp.Api.Model.Xsoar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IIonDataAccess
    {
        IonResponse GetIons(IonRequest request, bool isDownload = false);

        IonResponse GetAddOnsIons(IonRequest request, bool isDownload = false);

        DeviceIonTransferDetails GetIonDeviceTransferDetails(Device deviceId);

        void SaveIonDeviceLightDetails(DeviceIonDetailsLightInfo details, long userId);

        void TransferIonDeviceLicenses(DeviceIonTransferLicenses transfer, long userId);

        bool IsOnPremController(long supportAccountId);
    }
}