using System;
using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts
{
    public interface ISaasDataAccess
    {
        bool IsAuthCodeAlreadyRegistered(string authCode);
        SaasAuthCodeDetails GetAuthCodeDetails(string authCode);
        IEnumerable<AvailableSaaSActivations> GetAvailableSaasActivations(long SupportAccountID);
        IEnumerable<PanoramaSerialNumber> GetPanoramaSerialNumbers(long SupportAccountID);
        IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudService(long SupportAccountID, string mode, string serialNumber);
        bool IsHAPanorama(string serialNumber);
        string GetFreeLoggingService(string region, long supportAccountId);
        bool IsPanoramaAssociation(string authcode);
        bool IsEligibleForCdlPanoramaAssociation(string authcode);
        string GetLoggingServiceRegion(string authcode);
        EActivationActionType IsProductEligibleForActivation(string authcode, out string message);
        bool IsSeAuthCode(string authcode);
        bool IsCdlAuthCode(string authcode);
        List<BundleLicenseItem> GetBundlelicenses(string bundleLicenseIds);
        long GetBundlelicenseIdBySNAuthcode(string serialNumber, string authcode);
        bool VerifyAuthorizationBySerialNumberOrSupportAccountId(string email, string serialNumber, long supportAccountId);
        long GetPanoramaTenantByAppType(long supportAccountId, string serialNumber, AppPortalAppType appPortalApp);
        OTPOutput GetSaasOtpByType(long supportAccountId, string serialNumber, OtpType otpType);
        List<PanoramaSerialNumber> GetPrimaryPanorama(long supportAccountId);
        long GenerateTenantId(int retryLimit, bool isLoggingService);
        void CreateTenantForPanorama(long supportAccountId, long tenantId, string panoramaSerialNumber, string appInternalName, string userAccount);
        List<NameValueOnly> GetDevicesForOtp(long supportAccountId, long userAccountId, OtpDeviceType deviceType);
        IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudServiceForTransfer(long supportAccountID, string serialNumber);       
        string GetCDLTenantId(string authCode, long supportAccountID, string region);
    }
}
