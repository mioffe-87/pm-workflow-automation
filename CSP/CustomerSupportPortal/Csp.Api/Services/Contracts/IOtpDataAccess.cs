using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts
{
    public interface IOtpDataAccess
    {
        List<NameValueOnly> GetDevicesForOtp(long supportAccountId, long userAccountId, OtpDeviceType deviceType);
        bool AddUpdateOneTimePassword(OneTimePasswordModel otpModel, out DateTime? expirationDate);
        string GenerateOneTimePassword(string serialNumber);
        List<BulkOtpModel> GetPendingBulkOtpRequests();
        bool SaveBulkOtpRequest(BulkOtpModel bulkOtpRequest);
        List<DeviceOtp> GenerateBulkOtp(BulkOtpModel bulkOtp);
        bool SaveRegistrationPin(RegistrationPinModel registrationPinModel);
        OtpSummary<OtpSummaryItem> GetDeviceRegistrationOtpSummary(long supportAccountId, long userAccountId, DataParameters parameters, bool showHistory);
        OtpSummary<OtpPinSummaryItem> GetDeviceRegistrationPinSummary(long supportAccountId, long userAccountId, DataParameters parameters);
        int DisableDeviceRegistrationPin(long supportAccountId, long userAccountId, string ids);
        void ProcessOtpExpirationRequest(long userAccountId);
        void ProcessPinExpirationRequest(long userAccountId);
        bool EnableThermiteApiTest();
    }
}
