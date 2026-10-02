using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class DpmRepository : IDpmRepository {

		public string GetDevicePartNumber(string serialNumber) {

			using (var db = new cspEntities()) {

				var partNumber = db.devices.Where(d => d.SerialNumber == serialNumber).Select(d => d.partmaster.PartNumber).Single();

				return partNumber;
			}
		}

		private const string LookupTypeDpmEnabled = "DPMEnabled";

		public bool IsModelDpmEnabled(string model) {

			using (var db = new cspEntities()) {

				var isDpmEnabled = (
														 from l in db.lookups
														 where
															 l.LookupType == LookupTypeDpmEnabled
															 && l.LookupKey == model
														 select l).Any();

				return isDpmEnabled;
			}
		}

		private const string DecryptPartNumber = "-DECRYPT";

		public bool HasDpmLicense(string serialNumber) {

			using (var db = new cspEntities()) {

				var lics =
					from d in db.devices
					join l in db.licenses on d.DeviceId equals l.DeviceId
					where
						d.SerialNumber == serialNumber
						&& l.partmaster.PartNumber.Contains(DecryptPartNumber)
					select l;

				return lics.Any();
			}
		}
	}
}
