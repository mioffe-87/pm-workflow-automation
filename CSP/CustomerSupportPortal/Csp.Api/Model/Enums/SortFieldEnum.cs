using System.ComponentModel;

namespace Csp.Api.Model.Enums
{
    public enum SortFieldEnum
    {
        // wildfire
        [Description("portalName")]
        PortalName,

        // otp
        [Description("SerialNumber")]
        SerialNumber,
        [Description("DeviceType")]
        DeviceType,
        [Description("OTPType")]
        OTPType,
        [Description("Status")]
        Status,
        [Description("ExpirationDate")]
        ExpirationDate,
        [Description("Requestor")]
        Requestor,
        [Description("CreatedDate")]
        CreatedDate,

        // asset
        [Description("AssetTypeDisplayName")]
        AssetTypeDisplayName,
        [Description("Code")]
        Code,
        [Description("Name")]
        Name,
        [Description("DateRegistered")]
        DateRegistered,
        [Description("DatePurchased")]
        DatePurchased,

        // flex
        [Description("IsWarmFw")]
        IsWarmFw,
        [Description("VCpuQuantity")]
        VCpuQuantity,
        [Description("VMModel")]
        VMModel,
        [Description("AuthorizationCode")]
        AuthorizationCode,
        [Description("AscName")]
        AscName,
        [Description("DeviceName")]
        DeviceName,
        [Description("Licenses")]
        Licenses,
        [Description("DeploymentProfile")]
        DeploymentProfile,
        [Description("PanoramaModel")]
        PanoramaModel,
        [Description("CreditPoolId")]
        CreditPoolId,
        [Description("AvgTxtRecordsPerDay")]
        AvgTxtRecordsPerDay,
        [Description("max_threat_simulations")]
        MaxThreatSimulations,
        [Description("max_scans")]
        MaxScans,

        //ION
        [Description("authCode")]
        AuthCode,
        [Description("description")]
        Description,
        [Description("modelName")]
        ModelName,
        [Description("expiration")]
        Expiration,

        // Manage users
        [Description("firstName")]
        FirstName,
        [Description("lastName")]
        LastName,
        [Description("email")]
        Email,
        [Description("activationDate")]
        ActivationDate,
        [Description("roles")]
        Roles,

        // Groups
        [Description("DeviceTag")]
        DeviceTag,

        // ELA/ESA Decom History
        [Description("LastupdatedDate")]
        LastupdatedDate,
        [Description("Sku")]
        Sku,
        [Description("Model")]
        Model,
        [Description("AssetName")]
        AssetName,
        [Description("User")]
        User
    }
}
