using System;
using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts 
{
	public interface IVirtualMachineDataAccess 
    {
        bool IsAuthCodeAlreadyRegistered(EVmType vmType, string authCode);
        VmAuthCodeDetails GetAuthCodeDetails(EVmType vmType, string authCode);
		string GetNewStubSn(EVmType vmType);
        DateTime GetShipDate(EVmType vmType, string authCode);
        void CreateStubDevice(EVmType vmType, string authCode, string newStubSn, DateTime shipDate, long supportAccountId, long userAccountId, bool isVmElaStubDevice = false);
        DeviceRec GetDeviceDataFromAuthCode(EVmType vmType, string newVmAuthCode);
        DeviceRec GetDeviceDataFromLfid(EVmType vmType, long lfid);
        bool CanProvisionPanorama(EVmType vmType, string authCode);

		string GetPraSerialNumber(EVmType vmType);
		bool UpdateNewSerialNumber(EVmType vmType, string val, string newVal);

		IEnumerable<PraProvisioningRec> GetNsxProvisionedPanoramas(EVmType vmType, string authCode);
		bool UpdateAccountIdForPraDevice(EVmType vmType, string serialNum, long supportAccountId);
	    bool IsUpgradeComplete(EVmType vmType, long deviceId);
	    string RevokeVmElaLicense(EVmType vmType, string authCode, long supportAccountId);
        bool UpdateVmActivationUser(string serialNum, long userAccountId);

    }
}
