using System.Collections.Generic;
using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class WildfireRepository : IWildfireRepository {

		public void RemoveWildfireDevice(long deviceId) {

			using (var db = new cspEntities()) {

				var wildfireDevices = db.wildfiredevices.Where(x => x.DeviceId == deviceId).ToArray();

				var wildfireUserIds = wildfireDevices.Select(x => x.WildfireUserAccountId).ToArray();

				foreach (var wfDev in wildfireDevices) {
					db.wildfiredevices.Remove(wfDev);
					db.SaveChanges();
				}

				foreach (var wfUserId in wildfireUserIds) {
					var hasAnyOtherDevices = db.wildfiredevices.Any(x => x.WildfireUserAccountId == wfUserId);

					if (hasAnyOtherDevices) {
						continue;
					}

					var wfUser = db.wildfireuseraccounts.Single(x => x.WildfireUserAccountId == wfUserId);
					db.wildfireuseraccounts.Remove(wfUser);
					db.SaveChanges();
				}
			}
		}

		public string GetEmailByWildfireUserAccountId(long wildfireUserAccountId) {

			using (var db = new cspEntities()) {
				return db.wildfireuseraccounts.Single(x => x.WildfireUserAccountId == wildfireUserAccountId).Email;
			}
		}

		public string GetSerialNumberByDeviceId(long deviceId) {
			using (var db = new cspEntities()) {
				return db.devices.Single(x => x.DeviceId == deviceId).SerialNumber;
			}
		}

		public IEnumerable<long> GetExistingDevicesForUser(long wildfireUserAccountId, long supportAccountId) {

			using (var db = new cspEntities()) {
				return
					db.wildfiredevices.Where(
						x => x.WildfireUserAccountId == wildfireUserAccountId && x.SupportAccountId == supportAccountId)
						.Select(x => x.DeviceId)
						.ToArray();
			}
		}

        public string GetProductTypeByDeviceId(long deviceId)
        {
            string productType = string.Empty;
            using (var db = new cspEntities())
            {
                var subType = (from pst in db.partsubtypes
                               join pm in db.partmasters on pst.PartSubtypeId equals pm.PartSubtypeId
                               join l in db.licenses on pm.PartMasterId equals l.PartMasterId
                               where l.DeviceId == deviceId && l.IsActive && (!pst.LicenseType ?? false)
                               && (pst.PanDbIdentification.Contains("TRAPSMBETA") || pst.PanDbIdentification.Contains("MGFR")
                               || pst.PanDbIdentification.Contains("XDR") || pst.PanDbIdentification.Contains("TRAPS"))
                               && !pst.PanDbIdentification.Contains("MGFRXDRTRAPS")
                               select pst.PanDbIdentification).FirstOrDefault();

                subType = subType ?? string.Empty;

                if (subType.Contains("TRAPSMBETA"))
                    productType = "ANDROID";
                else if (subType.Contains("MGFR"))
                    productType = "MAGNIFIER";
                else if (subType.Contains("XDR"))
                    productType = "XDR";
                else if (subType.Contains("TRAPS"))
                    productType = "TRAPS";
                else 
                    productType = "WILDFIRE";

                return productType;
            }
        }

        public IEnumerable<long> GetExistingWFUsersForDevice(long deviceId)
        {

            using (var db = new cspEntities())
            {
                return
                    db.wildfiredevices.Where(
                        x => x.DeviceId == deviceId)
                        .Select(x => x.WildfireUserAccountId)
                        .ToArray();
            }
        }
    }
}
