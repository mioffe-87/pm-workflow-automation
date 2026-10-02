using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ISaasRegistrationRepository
    {
        IEnumerable<PanoramaSerialNumber> GetPanoramaSerialNumbers(long supportAccountId);
        bool UpdateSaasOTP(long saasOTPId, string updatedBy, string OTP, bool isActive);
        long InsertSaasOTP(string serialNumber, long supportAccountId, string lookUp, string createdBy);
        OTPOutput GetSaasOTP(long? supportAccountID, string serialNumber);
        long GetPanoramaTenantId(long supportAccountID, string serialNumber);
        bool HasLcaasLicense(string panoramaSerialNumber);
        bool ContainsSaasAssets(long deviceID, long supportAccountID);
        bool CheckIfPreviouslyActivated(string authCode, string panoramaSerialNumber);
        bool HasSaasBetaAccess(long supportAccountId);
        bool VerifyAuthorizationBySupportAccount(long supportAccountId, string email);
        bool VerifyAuthorizationBySerailNumber(string serialNumber, string email);
        bool IsTacUser(long supportAccountId, string email);
        bool IsTacUserOrAdmin(long supportAccountId, string email);
        IEnumerable<LoggingServiceDevice> GetLoggingServiceDevices(long supportAccountId);
        string GetAuthcodeType(string authCode);
    }
}
