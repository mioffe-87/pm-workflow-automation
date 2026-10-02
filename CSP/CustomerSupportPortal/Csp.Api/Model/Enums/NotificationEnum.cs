using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum NotificationDeliveryChannel
    {
        [Description("WEB")]
        Web,
        [Description("EMAIL")]
        Email,
        [Description("SMS")]
        Sms
    }

    public enum NotificationCategory 
    {
        [Description("USER_REGISTRATION")]
        UserRegistration,

        [Description("DEVICE_TRANSFER")]
        DeviceTransfer,

        [Description("VM_ELA_TOKEN_GRANT")]
        VMELATokenGrant,

        [Description("VM_ELA_TOKEN_UTILIZATION")]
        VMELATokenUtilization,

        [Description("CONSENT_REQUEST")]
        ConsentRequest,

        [Description("IMPERSONATE_CHANGE_SETTINGS")]
        ImpersonateSettingsChange,

        [Description("SUPER_USER_REMINDER")]
        SuperUserReminder
    }

    public enum NotificationSource
    {
        [Description("CSP")]
        Csp,
        [Description("IMPERSONATE")]
        Impersonate
    }

    public enum Topic
    {
        csp,
        notificationstest
    }
}
