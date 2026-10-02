using Newtonsoft.Json;
using System;
namespace Csp.Api.Model.Flex
{
    public class TransferDeploymentProfilesRequest
    {
        public long UserAccountId { get; set; }
        public long SupportAccountId { get; set; }
        public long DestCreditPool { get; set; }
        public string DestSupportType { get; set; }
        public string DestProductId { get; set; }
        public DateTime DestExpirationDate { get; set; }
        public DeploymentProfile deploymentProfile { get; set; }
        [JsonIgnore]
        public double TokensAllocated { get; set; }
        public string DestSku { get; set; }
        public long SourceCreditPool { get; set; }
        public string SourceCreditPoolGenType { get; set; }
        public string DestCreditPoolGenType { get; set; }


    }

    public class TransferDeploymentProfilesLicensingRequest
    {
        public string SourceAuthCode { get; set; }
        public long DestCreditPool { get; set; }
        public string DestSupportType { get; set; }
        public DateTime DestExpirationDate { get; set; }
        public long AscId { get; set; }
        public string AscName { get; set; }
        public string DestProductId { get; set; }
        public string Config { get; set; }
        public string PartId { get; set; }
    }
}
