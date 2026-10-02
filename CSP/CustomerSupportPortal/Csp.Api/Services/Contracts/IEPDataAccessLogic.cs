using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts
{
    public interface IEPDataAccessLogic
    {
        bool IsAuthCodeAlreadyRegistered(string authCode, string epType);
        bool IsEpLicense(string authCode, string epType);
        //string GetEpLicenseType(string authCode);
        void GetInfoToCreateStub(string authCode, out string licType,out string stubPartId, out string serialNum, out DateTime? shipDate);
        //long GetStubDeviceId(long supportAccountId);
        long GetStubDeviceId(long supportAccountId, string authCode, out string serialNumber);
        bool SaveStubDeviceIdToLicense(string authCode, long stubDeviceId, long supportAccountId, DateTime expiryDate);
        DateTime? GetExpiryDateForEpLicenseAuthCode(string authCode);
        bool SaveStubDeviceIdToLicense(string authCode, long stubDeviceId, long supportAccountId);
        //AvailableEp GetEndPointInfo(string authCode);
        string GetEndPointLicenseType(string authCode);
        string GetEndPointLicenseType(string authCode, out string stubPartId);
        DateTime? GetShipDateofEpAuthCode(string authCode);
        bool IsAuthCodeAlreadyRegistered(string authCode, long supportId);
        bool IsEpLicenseTypeAlreadyRegistered(long supportAccountId, string licType);
        bool IsEpLicenseTypeAlreadyRegistered(long supportAccountId, string licType, long stubId);
				IEnumerable<AvailableEp> GetAvailableEps(long supportAccountId, string searchField, string searchValue);
        DateTime? GetLicenseExpirationDateByEpStubId(long deviceId);
        long? GetEpDeviceInfo(string authCode, out string serialNumber, out string deviceName, out string panDBIdentification);
        long? GetLicenseCountByEpStubId(long deviceId);
        long? GetClientLicenseCountByEpStubId(long deviceId);
        long? GetServerLicenseCountByEpStubId(long deviceId);
				long? GetVdiLicenseCountByEpStubId(long deviceId);
        bool IsAuthCodeEp(string authCode);
        bool IsEpLicenseEsmOrBundle(string authCode);
        int GetNumberOfEsmTypesRegistered(long supportId);
        List<string> GetEndPointLicenseTypes(long stubDeviceId);
        List<EpLicenses> GetEpLicenses(long deviceId);
        long? GetDeviceIdForEpAuthCode(string authCode);

        string GetSerialNumberByDeviceId(long deviceId);
        bool SaveLicenseKey(string authCode, string licenseKey);
				bool GetLicenseKey(string authCode, long supportAccountId, out string licenseKey);

        void UpdateSupportAccountIdBySerialNumber(string serialNum, long supAccountId, long userAccountId);
        long GetDeviceIdBySerialNumber(string serialNumber);
        void UpdateDeviceRegistrationDate(string serialNumber, DateTime regDate);
        void RollBackPushToDeviceMasterForTraps(long deviceId, string authCode);
        void UpdateAccountIdBySerialNumber(string serialNum, long userAccountId);
				List<EpLicenseType> GetAllEndpointLicenseTypes(bool onlyShowEsmServerOrWs = false);

				bool EndPointHasSupport(long deviceId);
				bool EndPointHasBKLNSupport(long deviceId);
				bool EndPointHasBKLNSupport(string authorizationCode);

        bool IsBetaCustomer(string user);

        bool GetTrapsUseCount(string authCode, out int? workstationCount, out int? serverCount, out int? vdiCount,
            out int? agentCount);

        long? GetAgentLicenseCountByEpStubId(long deviceId);

    }
}
