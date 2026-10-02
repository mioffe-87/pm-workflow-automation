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
    public interface IIonLogic
    {
        IonResponse GetIons(IonRequest request, bool isDownload = false);

        DeviceIonTransferDetails GetIonDeviceTransferDetails(Device device);

        void SaveIonDeviceLightDetails(DeviceIonDetailsLightInfo details, long userId);

        void TransferIonDeviceLicenses(DeviceIonTransferLicenses transfer, long userId);

        dynamic GetIonsPageData(IonsPageRequest request);
    }
}