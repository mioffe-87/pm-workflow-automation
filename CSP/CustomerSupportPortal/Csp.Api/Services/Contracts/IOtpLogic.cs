using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts
{
    public interface IOtpLogic
    {
        List<NameValueOnly> GetDevicesForOtp(long supportAccountId, long userAccountId, OtpDeviceType deviceType);

        OTPOutput GetOneTimePassword(OneTimePasswordModel otpModel);

        bool SaveBulkOtpRequest(BulkOtpModel bulkOtpModel);

        List<BulkOtpModel> GetPendingBulkOtpRequests();

        void ProcessPendingBulkOtpRequests();

        void ProcessOtpExpirationRequest(long userAccountId);

        void ProcessPinExpirationRequest(long userAccountId);

        string DecodeBulkOtpPayload(string bulkOtpPayload);

        string EncodeBulkOtpPayload(string bulkOtpPayload);

        bool SaveRegistrationPin(RegistrationPinModel regPinModel);

        OtpSummary<OtpSummaryItem> GetDeviceRegistrationOtpSummary(long supportAccountId, long userAccountId, DataParameters parameters, bool showHistory = false);

        OtpSummary<OtpPinSummaryItem> GetDeviceRegistrationPinSummary(long supportAccountId, long userAccountId, DataParameters parameters);

        int DisableDeviceRegistrationPin(long supportAccountId, long userAccountId, string ids);

        bool EnableThermiteApiTest();

        byte[] GetOtpHistoryCsv(long supportAccountId, long userAccountId);

        CommonApiResponse<GenerateOtpResult> GenerateSingleOtp(SingleOtpRequest singleOtpRequest);
        CommonApiResponse<dynamic> GenerateBulkOtp(BulkOtpRequest singleOtpRequest);

    }
}
