using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Ion;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.Xsoar;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{
    public class IonLogic : BaseLogic, IIonLogic
    {
        private readonly IIonDataAccess ionDataAccess = null;

        public IonLogic(IIonDataAccess ionDataAccess)
        {
            this.ionDataAccess = ionDataAccess;
        }

        public IonResponse GetIons(IonRequest request, bool isDownload = false)
        {
            return request.DeviceType == EDeviceType.AddOns ? ionDataAccess.GetAddOnsIons(request, isDownload) : ionDataAccess.GetIons(request, isDownload);
        }

        public DeviceIonTransferDetails GetIonDeviceTransferDetails(Device device)
        {
            return ionDataAccess.GetIonDeviceTransferDetails(device);
        }

        public void SaveIonDeviceLightDetails(DeviceIonDetailsLightInfo details, long userId)
        {
            ionDataAccess.SaveIonDeviceLightDetails(details, userId);
        }

        public void TransferIonDeviceLicenses(DeviceIonTransferLicenses transfer, long userId)
        {
            ionDataAccess.TransferIonDeviceLicenses(transfer, userId);
        }

        public dynamic GetIonsPageData(IonsPageRequest request)
        {
            string onPremControllerLatestImageUrl = ConfigurationManager.AppSettings["OnPremControllerLatestImageUrl"];
            return new
            {
                showControllerTab = !string.IsNullOrWhiteSpace(onPremControllerLatestImageUrl),
                downloadControllerUpdateUrl = onPremControllerLatestImageUrl
            };
        }
    }
}