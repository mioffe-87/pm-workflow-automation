using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IEPLogic
    {
				IEnumerable<AvailableEp> GetAvailableEps(DataParameters parameters, long supportAccountId, string searchField, string searchValue, bool shouldGroupAuthCodes, bool isDebug = false);
				byte[] GetAvailableEpsCsv(long supportAccountId, string searchField, string searchValue);

        List<EpLicenses> GetEpLicenses(long deviceId);
        

        //AvailableEp GetEndPointInfo(string authCode);

        EpLicenseGenerationResult GenerateLicense(DateTime? expirationDate, string customerName,int? vdiLicenseCount, int? clientLicenseCount,
                                                  int? serverLicenseCount, int? agentLicenseCount, string wildfireApiKey,string serialNumber,
                                                  bool includeReflector, string licenseApiKey = null);
        AddEpAuthCodeResult AddEpAuthCode(string authCode, long supportAccountId, long userAccountId, string email = "");
        bool IsAuthCodeEsm(string authCode);
        bool IsAuthCodeEp100(string authCode);
        bool IsAuthCodeEp500(string authCode);
        bool IsAuthCodeAlreadyRegistered(string authCode, string epType);
        DateTime? GetLicenseExpirationDateByEpStubId(long deviceId);
        long? GetEpDeviceInfo(string authCode, out string serialNumber, out string deviceName, out string panDBIdentification);
        long? GetLicenseCountByEpStubId(long deviceId);
        bool IsTMSMalbork(string panDbIdentification);
        long? GetClientLicenseCountByEpStubId(long deviceId);
        long? GetServerLicenseCountByEpStubId(long deviceId);
				long? GetVdiLicenseCountByEpStubId(long deviceId);
                long? GetAgentLicenseCountByEpStubId(long deviceId);

        EpValidationResult ValidateAuthCode(string authCode, long supportId, long deviceId);
        EpValidationResult ValidateEsmAuthCode(string authCode, long supportId);
        AddEpAuthCodeResult AddEndpointLicense(string authCode, string esmAuthCode, long supportAccountId,
                                                 long userAccountId);
        long GetStubDeviceId(long supportAccountId, string authCode, out string serialNumber);

        bool SaveLicenseKey(string authCode, string licenseKey);
				bool GetLicenseKey(string authCode, long supportAccountId, out string licenseKey);

        bool IsEpLicenseEsmOrBundle(string authCode);

        bool IsBetaCustomer(string user);

        bool GetTrapsUseCount(string authCode, out int? workstationCount, out int? serverCount, out int? vdiCount,
            out int? trapsCount);

        string GetLicenseApiKey(long supportAccountId);
    }
}
