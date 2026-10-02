using Csp.Api.Model;
using Csp.Api.Model.Flex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IFirewallFlexApiLogic
    {
        dynamic GetCNFirewallSDevices(FlexDeviceRequest flexDeviceRequest, long userAccountId, long supportAccountId, string type, bool export = false, string subType = null);
        FileInfo DownloadAirgapLicense(long supportAccountId, long userAccountId, string authCode, bool withPanorama);
        dynamic RetryFailedDeploymentProfile(long deploymentProfileId);

        dynamic CreateFirewallFlexDeploymentProfile(FirewallFlexDeploymentProfile firewallFlexDeploymentProfile);

        dynamic UpdateFirewallFlexDeploymentProfile(FirewallFlexDeploymentProfile firewallFlexDeploymentProfile);

        dynamic GetEstimatedCost(TokenCalculatorRequest tokenCalculatorRequest, long supportAccountId, long userAccountId);
    }

}
