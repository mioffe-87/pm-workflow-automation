using System.ComponentModel;

namespace Csp.Api.Model.Enums
{
    public enum LicenseFeatureName
    {
        [Description("")]
        None,
        [Description("DPM")]
        Dpm,
        [Description("DPB")]
        Dpb,
        [Description("DNS")]
        Dns,
        [Description("SDWAN")]
        SdWan,
        [Description("GATEWAYTRIAL")]
        Gatewaytrial,
        [Description("IOTSecurity")]
        IOTSecurity,
        [Description("IOTSecurityDataLake")]
        IOTSecurityDataLake,
        [Description("IOTSecurity3rd")]
        IOTSecurity3rd,
        [Description("SaaSInline")]
        SaaSInline,

        [Description("AIOps")]
        AiOps
    }
}
