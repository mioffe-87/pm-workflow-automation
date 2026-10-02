using System.ComponentModel;
namespace Csp.Api.Model.Enums
{
    public enum DeploymentProfileType
    {
        [Description("VM")]
        VM = 1,

        [Description("CN")]
        CN = 2,

        [Description("AI")]
        AI = 3,

        [Description("CLOUD")]
        CLOUD = 4
    }

    public enum EDeploymentProfileSubType
    {
        [Description("VM")]
        VM = 1,

        [Description("API")]
        API = 2,

        [Description("RT")]
        RT = 3,

        [Description("MS")]
        MS = 4,

        [Description("GW")]
        GW = 7,

        [Description("APIM")]
        APIM = 8,

        [Description("AES")]
        AES = 9,

        [Description("CNGFW")]
        CNGFW = 10,

        [Description("SK")]
        SK = 11,

        [Description("AF")]
        AF = 12,

        [Description("SPM")]
        SPM = 13,

    }

    public enum PoolRenewalStatus
    {
        None,
        Confirmed,
        Active
    }


    public enum ConsumptionRenewalStatus
    {
        None,
        Confirmed,
        AutoRenewed,
        Renewed
    }

    public enum CreditPoolStatus
    {
        Active,
        Inactive,
        Expired,
        Suspended,
        Cancelled
    }
}
