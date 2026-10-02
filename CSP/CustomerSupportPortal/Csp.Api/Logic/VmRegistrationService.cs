using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic 
{
	public class VmRegistrationService : IVmRegistrationService 
    {
		//private readonly IVmRegistrationRepository _vmRegistrationRepository;
		//private readonly IFileLogger _fileLogger;

        private readonly IVmRegistrationDataAccess _vmRegistrationDataAccess;


        //public VmRegistrationService(IVmRegistrationRepository vmRegistrationRepository, IFileLogger fileLogger, IVmRegistrationDataAccess vmRegistrationDataAccess) 
        public VmRegistrationService(IVmRegistrationDataAccess vmRegistrationDataAccess)
        {
			//_vmRegistrationRepository = vmRegistrationRepository;
			//_fileLogger = fileLogger;
            _vmRegistrationDataAccess = vmRegistrationDataAccess;
		}

        public void CorrectVmRegistrationData(EVmType vmType)
        {
            //Get registered, active devices without support accounts
            var devs = _vmRegistrationDataAccess.GetRegisteredDevicesWithoutSupportAccountIds(vmType);
            foreach (var deviceId in devs)
            {
                //see if the user who registered them belongs to 1 and only 1 account and that they are not an ASC or TAC user
                //If that works, then use their account
                var supportAccountId = _vmRegistrationDataAccess.GetRegisteringUserSupportAccountIfOnlyOne(vmType, deviceId);

                if (supportAccountId.HasValue)
                {

                    _vmRegistrationDataAccess.SetDeviceSupportAccountId(vmType, deviceId, supportAccountId.Value);
                    //_fileLogger.AppendLine(string.Format("UPDATE device SET SupportAccountID = {0} WHERE DeviceId = {1}", supportAccountId.Value, deviceId));
                    continue;
                }

                //Otherwise, check if there is a vmparentlicense or vmstubid
                //if so, see if any devs/lics sharing those ids have a registered support account and use that
                supportAccountId = _vmRegistrationDataAccess.GetRelatedVmSupportAccountIfExists(vmType, deviceId);
                if (supportAccountId.HasValue)
                {

                    _vmRegistrationDataAccess.SetDeviceSupportAccountId(vmType, deviceId, supportAccountId.Value);
                    //_fileLogger.AppendLine(string.Format("UPDATE device SET SupportAccountID = {0} WHERE DeviceId = {1}", supportAccountId.Value, deviceId));
                    continue;
                }
            }
        }

        //public void CorrectVmRegistrationData(EVmType vmType)
        //{

        //    Get registered, active devices without support accounts

        //    var devs = _vmRegistrationRepository.GetRegisteredDevicesWithoutSupportAccountIds(vmType);

        //    foreach (var deviceId in devs)
        //    {

        //        see if the user who registered them belongs to 1 and only 1 account and that they are not an ASC or TAC user
        //        If that works, then use their account
        //        var supportAccountId = _vmRegistrationRepository.GetRegisteringUserSupportAccountIfOnlyOne(vmType, deviceId);

        //        if (supportAccountId.HasValue)
        //        {

        //            _vmRegistrationRepository.SetDeviceSupportAccountId(vmType, deviceId, supportAccountId.Value);
        //            _fileLogger.AppendLine(string.Format("UPDATE device SET SupportAccountID = {0} WHERE DeviceId = {1}", supportAccountId.Value, deviceId));
        //            continue;
        //        }


        //        Otherwise, check if there is a vmparentlicense or vmstubid

        //        if so, see if any devs / lics sharing those ids have a registered support account and use that
        //          supportAccountId = _vmRegistrationRepository.GetRelatedVmSupportAccountIfExists(vmType, deviceId);
        //        if (supportAccountId.HasValue)
        //        {

        //            _vmRegistrationRepository.SetDeviceSupportAccountId(vmType, deviceId, supportAccountId.Value);
        //            _fileLogger.AppendLine(string.Format("UPDATE device SET SupportAccountID = {0} WHERE DeviceId = {1}", supportAccountId.Value, deviceId));
        //            continue;
        //        }
        //    }
        //}
    }
}
