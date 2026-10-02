using System.Collections.Generic;

namespace Csp.Api.Services.Contracts {
	public interface IWildfireRepository {
		void RemoveWildfireDevice(long deviceId);
		string GetEmailByWildfireUserAccountId(long wildfireUserAccountId);
		string GetSerialNumberByDeviceId(long deviceId);
		IEnumerable<long> GetExistingDevicesForUser(long wildfireUserAccountId, long supportAccountId);
        string GetProductTypeByDeviceId(long deviceId);
        IEnumerable<long> GetExistingWFUsersForDevice(long deviceId);
    }
}
