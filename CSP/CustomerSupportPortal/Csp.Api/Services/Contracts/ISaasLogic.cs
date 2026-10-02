using System;
using System.Collections.Generic;
using System.Web;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts 
{
    public interface ISaasLogic
    {
        IEnumerable<AvailableSaaSActivations> GetAvailableSaasActivations(DataParameters parameters, long supportAccountId, string searchBy, string searchValue, long userAccountId,bool isAscAccount = false);

       
        AddSaasAuthCodeResult AddSaasAuthCode(string authCode, string panoramaSerialNumber, long supportAccountId, long userAccountId, string loggingRegion, bool IsPanoramaAssociation);

        byte[] GetAvailableSaasCsv(long supportAccountId, string searchBy, string searchValue,long userAccountId);

        IEnumerable<PanoramaSerialNumber> GetPanoramaSerialNumbers(long supportAccountId);

        bool UpdateSaasOTP(long saasOTPId, string updatedBy, string OTP, bool isActive);

        long InsertSaasOTP(string serialNumber, long supportAccountId, string lookUp, string createdBy);

        OTPOutput GetSaasOTP(long? supportAccountID, string serialNumber, OtpType otpType = OtpType.LoggingService);

        long GetPanoramaTenantId(long supId, string serialNumber, AppPortalAppType appType = AppPortalAppType.LoggingService);

        List<PanoramaSerialNumber> GetPrimaryPanorama(long supportAccountId);

        IEnumerable<LoggingRegions> GetLoggingRegions();

        IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudService(long SupportAccountID, string mode, string serialNumber);

        bool? HasLcaasLicense(string authCode, string panoramaSerialNumber);

        bool CheckIfPreviouslyActivated(string authCode, string panoramaSerialNumber);

        bool VerifyAuthorizationBySupportAccount(long supportAccountId, string email);

        bool VerifyAuthorizationBySerailNumber(string serialNumber, string email);

        AuthCodeType GetCloudServiceAuthCodeType(string authcode);

        IEnumerable<LoggingServiceDevice> GetLoggingServiceDevices(long supportAccountId);

        bool HasBetaAccess(long supportAccountId);

        bool IsHAPanorama(string serialNumber);

        string GetFreeLoggingService(string region, long supportAccountId);

        bool IsPanoramaAssociation(string authcode);

        bool IsEligibleForCdlPanoramaAssociation(string authcode);

        EActivationActionType IsProductEligibleForActivation(string authcode, out string message);

        bool IsSeAuthCode(string authcode);

        bool IsCdlAuthCode(string authcode);

        EasyOnboardingAPIResponse GetCdlMagicLink(string authCode);

        long GenerateTenantId(int retryLimit, bool isLoggingService);

        void CreateZtpTenantForPanorama(long supportAccountId, string panoramaSerialNumber, string userAccount);

        List<NameValueOnly> GetDevicesForOtp(long supportAccountId, long userAccountId, OtpDeviceType deviceType);

        IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudServiceForTransfer(long supportAccountID, string serialNumber);
        
    }
}
