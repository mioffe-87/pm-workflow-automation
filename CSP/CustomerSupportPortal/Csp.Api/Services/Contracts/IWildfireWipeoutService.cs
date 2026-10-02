using System.Collections.Generic;

namespace Csp.Api.Services.Contracts {
	public interface IWildfireWipeoutService {
		bool WildFireWipeout(string serialnumber);
		object ReplaceEmailNotification(string oldSerialNumber, string newSerialNumber);
		bool DeleteEmailNotification(long wildfireUserAccountId, long supportAccountId, long? deviceId);
		bool DeleteEmailNotificationForDevices(long wildfireUserAccountId, long supportAccountId, IEnumerable<long> newDeviceIds);
	}
}
