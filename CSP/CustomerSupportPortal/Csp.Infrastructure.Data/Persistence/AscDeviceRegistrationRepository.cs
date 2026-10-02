using System;
using System.Linq;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class AscDeviceRegistrationRepository : IAscDeviceRegistrationRepository {
		public bool IsAsc(long supportAccountId) {
			using (var db = new cspEntities()) {
				var accountType = (
														from sa in db.supportaccounts
														where sa.SupportAccountId == supportAccountId
														select sa.accounttype.Name).FirstOrDefault();

				return accountType == "ASC";
			}
		}

		public bool IsSupportMemberOfSupportAccount(long userAccountId, long supportAccountId) {
			const short ascSecurityClassification = (short)SecurityClassification.Support;

			using (var db = new cspEntities()) {
				var activeMemberships =
					from mem in db.memberships
					join mr in db.membershiproles on mem.MembershipId equals mr.MembershipId
					where
						mem.UserAccountId == userAccountId
						&& mem.SupportAccountId == supportAccountId
						&& mem.ActivationDate <= DateTime.Now
						&& (
								 mem.ExpirationDate == null
								 || mem.ExpirationDate >= DateTime.Now
							 )
						&& mr.role.SecurityClassification == ascSecurityClassification
					select mem;

				return activeMemberships.Any();
			}
		}

		public long[] GetDeviceSalesOrderSupportAccountId(string serialNumber) {
			using (var db = new cspEntities()) {
				var supportAccountIds =
					from soitem in db.salesorderitems
					join so in db.salesorders on soitem.SalesOrderId equals so.SalesOrderId
					join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
					where soitem.device.SerialNumber == serialNumber
					select map.SupportAccountId;

				return supportAccountIds.ToArray();
			}
		}

		public bool HasBklnSupportInSalesOrder(string serialNumber, long supportCenterSupportAccountId) {
			using (var db = new cspEntities()) {

				var linkedSapIds =
					(
						from soitem in db.salesorderitems
						join so in db.salesorders on soitem.SalesOrderId equals so.SalesOrderId
						join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
						join mapSap in db.sapsupportaccountmappings on map.SupportAccountId equals
							mapSap.SupportAccountId
						where
							soitem.device.SerialNumber == serialNumber
						select (long?)mapSap.SAPId
					).ToArray();

				var bklnLics =
					from soitem in db.salesorderitems
					join so in db.salesorders on soitem.SalesOrderId equals so.SalesOrderId
					where
					linkedSapIds.Contains(so.SAPCustomerId)
					&& (
					soitem.license.partmaster.PartNumber.Contains("bkln")
					|| soitem.license.partmaster.PartNumber.Contains("b4hr")
					)
					&& soitem.license.SupportCenterSupportAccountId == supportCenterSupportAccountId
					select soitem;

				return bklnLics.Any();
			}
		}

		public bool IsRegisteredAuthCode(string authCode) {
			using (var db = new cspEntities()) {
				var isRegistered =
					(
						from lic in db.licenses
						where lic.AuthorizationCode == authCode
									&& lic.DeviceId != null
									&& lic.RegistrationDate != null
						select lic).Any();

				return isRegistered;
			}
		}

		public bool IsBklnAuthCode(string authCode) {
			using (var db = new cspEntities()) {
				var isBlkn =
					(
						from lic in db.licenses
						where lic.AuthorizationCode == authCode
									&& (
											 lic.partmaster.PartNumber.Contains("bkln")
											 || lic.partmaster.PartNumber.Contains("b4hr")
										 )
						select lic).Any();

				return isBlkn;
			}
		}

		public bool HasAnyMemberships(long supportAccountId) {
			using (var db = new cspEntities()) {
				return db.memberships.Any(x => x.SupportAccountId == supportAccountId);
			}
		}

		public long GetBklnSupportCenterSupportAccountId(string authCode) {
			using (var db = new cspEntities()) {
				return db.licenses.Single(x => x.AuthorizationCode == authCode).SupportCenterSupportAccountId.GetValueOrDefault();
			}
		}

		public void UpdateForDeviceRegistrationWithoutMembershipCheck(string serialNumber, string region, long? categoryId,
																																	long lastUpdateByUserAccountId, long supportAccountId) {
			using (var db = new cspEntities()) {

				var device = db.devices.Single(x => x.SerialNumber == serialNumber);

				device.SupportAccountId = supportAccountId;
				device.Region = region;
				device.DeviceCategoryId = categoryId;
				device.LastUpdate = DateTime.Now;
				device.LastUpdateByUserAccountId = lastUpdateByUserAccountId;

				db.SaveChanges();
			}
		}
	}
}
