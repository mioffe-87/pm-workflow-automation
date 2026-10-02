using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Wildfire;

namespace Csp.Api.Services.Contracts
{
    public interface IWildfireLogic
    {
        WildfireAssetsResponse<WildfireAssetItem> GetApiAssets(WildfireApiRequest request);

        WildfireAssetsResponse<WildfirePortalItem> GetPortals(WildfireApiRequest request);

        WildfireApiActivationDetailsResponse GetActivationDetails(WildfireApiActivationDetailsRequest request);

        WildfireActivationLicense GetProductTypeBasedDetails(WildfireApiActivationDetailsRequestData data);

        WildfireApiActivateResponse Activate(WildfireApiActivateRequest request);

        dynamic GetWildfireKeys(long supportAccountId);

        WfApiConsumptionHealh CheckConsumptionHealth();

        WfApiConsumptionSettings GetConsumptionSettings(long supportAccountId);

        WfApiConsumptionResponse GetConsumptionData(WfApiConsumptionRequest request);
    }
}