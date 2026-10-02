using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class UserRegistrationRepository : IUserRegistrationRepository {

		public bool IsEmailInUse(string email) {
			using (var db = new cspEntities()) {
				return db.useraccounts.Any(x => x.Email == email);
			}
		}

		public bool IsDisplayNameInUse(string displayName) {
			using (var db = new cspEntities()) {
				return db.useraccounts.Any(x => x.DisplayName == displayName);
			}
		}

		public IEnumerable<long> GetSalesOrderSupportAccountIdsByDeviceSerialNumber(string serialNumber) {
			using (var db = new cspEntities()) {
				var supportAccountIds =
					from dev in db.devices
					join soitems in db.salesorderitems on dev.DeviceId equals soitems.DeviceId
					join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId
					join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
					where dev.SerialNumber == serialNumber
					select map.SupportAccountId;

				return supportAccountIds.ToArray();
			}
		}

		public IEnumerable<long> GetSalesOrderSupportAccountIdsByLicenseAuthCode(string authCode) {
			using (var db = new cspEntities()) {
				var supportAccountIds =
					from lic in db.licenses
					join soitems in db.salesorderitems on lic.LicenseId equals soitems.LicenseId
					join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId
					join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
					where lic.AuthorizationCode == authCode
					select map.SupportAccountId;

				return supportAccountIds.ToArray();
			}
		}

		public long[] GetSalesOrderSupportAccountIdsBySapId(string sapId) {

			long sapIdNum;
			if (!long.TryParse(sapId, out sapIdNum)) {
				return new long[0];
			}

			using (var db = new cspEntities()) {
				var supportAcocuntIds =
					from map in db.sapsupportaccountmappings
					where map.SAPId == sapIdNum
					select map.SupportAccountId;

				return supportAcocuntIds.ToArray();
			}
		}

		public long[] GetSalesOrderSupportAccountIdsBySalesOrderNumber(string salesOrderNumber) {

			var paddedSalesOrderNumber = salesOrderNumber.PadLeft(10, '0');

			using (var db = new cspEntities()) {
				var supportAcocuntIds =
					from so in db.salesorders
					join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
					where so.SAPSalesOrderNumber == paddedSalesOrderNumber
					select map.SupportAccountId;

				return supportAcocuntIds.ToArray();
			}
		}

		public long[] GetSalesOrderSapIdsBySalesOrderNumber(string salesOrderNumber) {
			var paddedSalesOrderNumber = salesOrderNumber.PadLeft(10, '0');

			using (var db = new cspEntities()) {
				var sapIds =
					from so in db.salesorders
					where so.SAPSalesOrderNumber == paddedSalesOrderNumber
					select so.SAPCustomerId.Value;

				return sapIds.ToArray();
			}
		}

		public long[] GetSupportAccountIdsBySfAccountId(string sfAccountId, string evalId) {
			using (var db = new cspEntities()) {

				//First check if salesforce has logged a mapping for this eval
				var mapping = (
											 from map in db.evalsfdcmappings
											 where map.EvalId == evalId
											 select map).SingleOrDefault();

				if (mapping != null) {
					return new[] { mapping.SupportAccountId };
				}

				//If not, then try to find any matches
				var supportAccountIds =
					from sa in db.supportaccounts
					where sa.IsActive == true
						&& sa.SalesforceAccountId.Substring(0, 15) == sfAccountId.Substring(0, 15)
					select sa.SupportAccountId;

				return supportAccountIds.ToArray();
			}
		}

		public bool IsValidDeviceSerialNumber(string serialNumber) {
			using (var db = new cspEntities()) {
				var deviceFound = db.devices.Any(x => x.SerialNumber == serialNumber);

				return deviceFound;
			}
		}

		public bool IsValidLicenseAuthCode(string authCode) {
			using (var db = new cspEntities()) {
				var licenseFound = db.licenses.Any(x => x.AuthorizationCode == authCode);

				return licenseFound;
			}
		}

		public bool IsEvalDeviceCode(string deviceCode) {
			using (var db = new cspEntities()) {
				const string evalPanDbIdentification = "EVAL";
				if (deviceCode == null) {
					return false;
				}
				var hasEvalLic = (
									 from l in db.licenses
									 where
										 l.AuthorizationCode == deviceCode
										 && l.partmaster.partsubtype.PanDbIdentification.Contains(evalPanDbIdentification)
									 select l
								 ).Any();

				if (hasEvalLic) {
					return true;
				}

				var data = (
										 from dev in db.devices
										 where
											 dev.SerialNumber == deviceCode
										 select new {
											 pandbid = dev.partmaster.partsubtype.PanDbIdentification ?? string.Empty,
											 partnumber = dev.partmaster.PartNumber,
										 }
									 ).SingleOrDefault();

				if (data == null) {
					return false;
				}

				var isEval = data.pandbid.Contains(evalPanDbIdentification);

				if (isEval) return true;

				isEval = Regex.Match(data.partnumber, @"-E\d{2}$").Success;

				return isEval;
			}
		}

	    public SelfRegistrationCodeModel GetSelfRegistrationByCode(string code)
	    {
	        var db = SessionDbContext.CspObjectContext;

	        var selfRegCode = db.selfregistrationcodes.SingleOrDefault(x => x.Code == code);

	        if (selfRegCode == null)
	        {
	            return null;
	        }

	        return new SelfRegistrationCodeModel
	        {
                Code = code,
                SupportAccountId = selfRegCode.SupportAccountId,
                IsActive = selfRegCode.IsActive,
                ExpirationDate = selfRegCode.ExpirationDate,
	        };
	    }
	}
}
