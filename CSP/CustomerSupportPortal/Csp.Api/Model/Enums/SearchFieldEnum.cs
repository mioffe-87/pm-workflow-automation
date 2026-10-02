using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum SearchFieldEnum
    {
        // Production devices
        [Description("SerialNumber")]
        SerialNumber,
        [Description("AuthorizationCodes")]
        AuthorizationCodes,
        [Description("ExpiresBy")]
        ExpiresBy,
        [Description("ModelName")]
        ModelName,
        [Description("DeviceName")]
        DeviceName,
        [Description("ASCName")]
        ASCName,
        [Description("DeviceCategoryId")]
        DeviceCategoryId,
        [Description("Comments")]
        Remarks,

        // Flex devices
        [Description("VMModel")]
        VMModel,
        [Description("LastCheckinDate")]
        LastCheckinDate,
        [Description("PanoramaModel")]
        PanoramaModel,
        [Description("AuthorizationCode")]
        AuthorizationCode,

        //ION
        [Description("authCode")]
        AuthCode,

        // Manage users
        [Description("firstName")]
        FirstName,
        [Description("lastName")]
        LastName,
        [Description("email")]
        Email,
        [Description("expirationDate")]
        ExpirationDate,
        [Description("activationDate")]
        ActivationDate,
        [Description("lastLoginDate")]
        LastLoginDate,
        [Description("description")]
        Description,
        [Description("roles")]
        Roles,
        [Description("comments")]
        Comments,

        // Groups
        [Description("DeviceTag")]
        DeviceTag,

    }
}
