using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum CronJobEnum
    {
        EvalAccountCreation = 1,
        VmRegistration = 2,
        SfdcAddUpdateContact = 3,
        BulkRegistration = 4,
        AutofocusTrialExp = 5,
        MfaRegistration = 6,
        BulkDeviceTransfer = 7,
        EmailNotifications = 8,
        LicensingApiNotification = 9,
        VmElaTokenNotification = 10,
        CloudServiceUserTransfer = 11,
        BulkOtpRequest = 12,
        OtpExpirationRequest = 13,
        PinExpirationRequest = 14
    }

    public enum CronJobStatus
    {
        Pending = 1,
        Success = 2,
        Failed = 3
    }
}
