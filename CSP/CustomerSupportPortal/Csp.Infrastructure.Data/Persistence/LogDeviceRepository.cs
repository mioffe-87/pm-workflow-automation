using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;

namespace Csp.Infrastructure.Data.Persistence {

	public class LogDeviceRepository : ILogDeviceRepository {
		private readonly IRepository _repository;

		public LogDeviceRepository(IRepository repository) {
			_repository = repository;
		}


		public IEnumerable<TransferLicenseAvailableDevice> SearchArchivedDevices(long restrictedByUserAccountId, long supportAccountId, string searchText) {

			// Build the security query
			var ascSubQuery = AscSubQuery(restrictedByUserAccountId, supportAccountId);

			var defaultGroupsSubQuery = DefaultGroupsSubQuery(restrictedByUserAccountId, supportAccountId);

			var defaultSubQuery = DefaultSubQuery(restrictedByUserAccountId, supportAccountId);

			var visibleDevicesQuery = ascSubQuery.Union(defaultGroupsSubQuery).Union(defaultSubQuery);

			var isGlobalOrAdmin = IsGlobalOrAdmin(restrictedByUserAccountId, supportAccountId);

			if (isGlobalOrAdmin) {

				var globalOrAdminSubQuery = GlobalOrAdminSubQuery(supportAccountId);

				visibleDevicesQuery = visibleDevicesQuery.Union(globalOrAdminSubQuery);
			}

			var allowedDeviceIds = visibleDevicesQuery.Select(x => x.DeviceId).ToArray();

			var result =
				from dev in _repository.Query<Api.EntityClasses.Logdevice>()
                join lic in _repository.Query<Api.EntityClasses.License>() on dev.DeviceId equals lic.DeviceId.Value
				where
					allowedDeviceIds.Contains(dev.DeviceId)
					&& (
						dev.IsActive == 1
						|| (dev.IsRmareturn == 1 && dev.IsRmareplacement == 0)
					)
					&& (
						dev.SerialNumber.ToUpper().Contains(searchText.ToUpper())
						|| dev.DeviceName.ToUpper().Contains(searchText.ToUpper())
					)
                    && (!lic.AuthorizationCode.StartsWith("E"))
				select new TransferLicenseAvailableDevice {
					DeviceId = dev.DeviceId,
					PartNumber = dev.PartMasterId_Partmaster_.PartNumber,
					SerialNumber = dev.SerialNumber,
				};

			return result.ToArray().Distinct().ToArray();
		}

		private IEnumerable<ArchivedDeviceModel> GlobalOrAdminSubQuery(long supportAccountId) {

			var globalOrAdminSubQuery =
					from dev in _repository.Query<Api.EntityClasses.Logdevice>()

					where
						dev.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
						&& dev.PartMasterId_Partmaster_.PartTypeId_Parttype_.PanDbIdentification == PanDbIds.Dev
						&& (
							dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_ == null
							|| (
								dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Oss
								&& dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Spare
							)
						)

					select new ArchivedDeviceModel {
						DeviceId = dev.DeviceId,
					};

			return globalOrAdminSubQuery.ToArray();
		}

		private IEnumerable<ArchivedDeviceModel> AscSubQuery(long restrictedByUserAccountId, long supportAccountId) {

			var ascSubQuery =
				from dev in _repository.Query<Api.EntityClasses.Logdevice>()
				join mem in _repository.Query<Api.EntityClasses.Membership>() on
					dev.SupportCenterSupportAccountId_Supportaccount_.SupportAccountId equals
					mem.SupportAccountId_Supportaccount_.SupportAccountId
				join memrol in _repository.Query<Api.EntityClasses.Membershiprole>() on mem.MembershipId equals memrol.MembershipId_Membership_.MembershipId
				join lic in _repository.Query<Api.EntityClasses.License>() on dev.DeviceId equals lic.DeviceId.Value

				where
					lic.SupportCenterSupportAccountId == dev.SupportCenterSupportAccountId_Supportaccount_.SupportAccountId
					&& dev.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					&& mem.UserAccountId_Useraccount_.UserAccountId == restrictedByUserAccountId
					&& memrol.RoleId_Role_.SecurityClassification == (int)SecurityClassification.Support
					&& mem.ActivationDate <= DateTime.Now
					&& (
							 mem.ExpirationDate >= DateTime.Now
							 || mem.ExpirationDate.HasValue == false
						 )
					&& (
						lic.IsActive == 1 
					)
					&& dev.PartMasterId_Partmaster_.PartTypeId_Parttype_.PanDbIdentification == PanDbIds.Dev
					&& (
						dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_ == null
						|| (
							dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Oss
							&& dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Spare
						)
					)

				select new ArchivedDeviceModel {
					DeviceId = dev.DeviceId,
				};

			return ascSubQuery.ToArray();
		}

		private IEnumerable<ArchivedDeviceModel> DefaultGroupsSubQuery(long restrictedByUserAccountId, long supportAccountId) {

			var defaultGroupsSubQuery =
				from dev in _repository.Query<Api.EntityClasses.Logdevice>()
				join mem in _repository.Query<Api.EntityClasses.Membership>() on
					dev.SupportCenterSupportAccountId_Supportaccount_.SupportAccountId equals
					mem.SupportAccountId_Supportaccount_.SupportAccountId
				join memrol in _repository.Query<Api.EntityClasses.Membershiprole>() on mem.MembershipId equals memrol.MembershipId_Membership_.MembershipId

				join sag in _repository.Query<Api.EntityClasses.Supportaccountgroup>() on
					new { dev.SupportAccountId_Supportaccount_.SupportAccountId, mem.GroupId_Supportaccountgroup_.GroupId } equals
					new { sag.SupportAccountId_Supportaccount_.SupportAccountId, sag.GroupId }

				join grpdev in _repository.Query<Api.EntityClasses.Groupdevice>() on
					new { sag.GroupId, dev.DeviceId } equals
					new { grpdev.GroupId_Supportaccountgroup_.GroupId, grpdev.DeviceId_Device_.DeviceId }

				where
					dev.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					&& mem.UserAccountId_Useraccount_.UserAccountId == restrictedByUserAccountId
					&& memrol.RoleId_Role_.SecurityClassification == (int)SecurityClassification.Default
					&& mem.ActivationDate <= DateTime.Now
					&& (
							 mem.ExpirationDate >= DateTime.Now
							 || mem.ExpirationDate.HasValue == false
						 )
					&& dev.PartMasterId_Partmaster_.PartTypeId_Parttype_.PanDbIdentification == PanDbIds.Dev
					&& (
						dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_ == null
						|| (
							dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Oss
							&& dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Spare
						)
					)

				select new ArchivedDeviceModel {
					DeviceId = dev.DeviceId,
				};

			return defaultGroupsSubQuery.ToArray();
		}

		private IEnumerable<ArchivedDeviceModel> DefaultSubQuery(long restrictedByUserAccountId, long supportAccountId) {

			var excludeGroupDevices =
				from grpdev in _repository.Query<Api.EntityClasses.Groupdevice>()
				where grpdev.GroupId_Supportaccountgroup_.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
				select grpdev.DeviceId_Device_.DeviceId;


			var defaultSubQuery =
				from dev in _repository.Query<Api.EntityClasses.Logdevice>()
				join mem in _repository.Query<Api.EntityClasses.Membership>() on
					dev.SupportCenterSupportAccountId_Supportaccount_.SupportAccountId equals
					mem.SupportAccountId_Supportaccount_.SupportAccountId
				join memrol in _repository.Query<Api.EntityClasses.Membershiprole>() on mem.MembershipId equals memrol.MembershipId_Membership_.MembershipId

				where
					!excludeGroupDevices.Contains(dev.DeviceId) // TODO - Convert this "not in" to a left join to speed up query (use nhibernate query language)
					&& dev.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					&& mem.UserAccountId_Useraccount_.UserAccountId == restrictedByUserAccountId
					&& memrol.RoleId_Role_.SecurityClassification == (int)SecurityClassification.Default
					&& mem.ActivationDate <= DateTime.Now
					&& (
							 mem.ExpirationDate >= DateTime.Now
							 || mem.ExpirationDate.HasValue == false
						 )
					&& dev.PartMasterId_Partmaster_.PartTypeId_Parttype_.PanDbIdentification == PanDbIds.Dev
					&& (
						dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_ == null
						|| (
							dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Oss
							&& dev.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.PanDbIdentification != PanDbIds.Spare
						)
					)

				select new ArchivedDeviceModel {
					DeviceId = dev.DeviceId,
				};

			return defaultSubQuery.ToArray();
		}

		private bool IsGlobalOrAdmin(long restrictedByUserAccountId, long supportAccountId) {

			return
				HasGlobalClassificationForAccount(restrictedByUserAccountId, supportAccountId)
				|| HasAdminClassificationForAccount(restrictedByUserAccountId, supportAccountId);
		}

		private bool HasGlobalClassificationForAccount(long restrictedByUserAccountId, long supportAccountId) {

			var globalMemberships =
				from memrol in _repository.Query<Api.EntityClasses.Membershiprole>()
				where
					memrol.RoleId_Role_.SecurityClassification == (int)SecurityClassification.Global
					&& memrol.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId == restrictedByUserAccountId
					&& memrol.MembershipId_Membership_.SupportAccountId_Supportaccount_.SupportAccountId != supportAccountId
					&& memrol.MembershipId_Membership_.ActivationDate <= DateTime.Now
					&& (
						memrol.MembershipId_Membership_.ExpirationDate >= DateTime.Now
						|| memrol.MembershipId_Membership_.ExpirationDate.HasValue == false
					)
				select memrol.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId;

			return globalMemberships.Any();
		}

		private bool HasAdminClassificationForAccount(long restrictedByUserAccountId, long supportAccountId) {

			var adminMemberships =
				from memrol in _repository.Query<Api.EntityClasses.Membershiprole>()
				where
					memrol.RoleId_Role_.SecurityClassification == (int)SecurityClassification.Administrative
					&& memrol.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId == restrictedByUserAccountId
					&& memrol.MembershipId_Membership_.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					&& memrol.MembershipId_Membership_.ActivationDate <= DateTime.Now
					&& (
						memrol.MembershipId_Membership_.ExpirationDate >= DateTime.Now
						|| memrol.MembershipId_Membership_.ExpirationDate.HasValue == false
					)
				select memrol.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId;

			return adminMemberships.Any();
		}
	}
}
