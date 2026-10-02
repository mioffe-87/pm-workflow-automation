using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface IGroupLogic {
		Group GetGroup(long groupId, long restrictedByUserAccountId);
		void CreateGroupDevice(long groupId, long deviceId, long createdByUserAccountId);
		Group UpdateGroupNameAndDescription(Group group, long restrictedByUserAccountId);
		Group CreateGroup(Group group);
		void DeleteGroup(long groupId, long deletedByUserAccountId);
		List<GroupListItem> GetSupportAccountGroups(long supportAccountId, long restrictedByUserAccountId,
																								List<RoleSecurity> roleSecurities, DataParameters parameters);

		List<GroupListItem> GetDeviceGroups(long deviceId, long restrictedByUserAccountId, DataParameters parameters);
		void DeleteGroupDevice(long groupId, long deviceId, long deletedByUserAccountId);
		List<GroupListItem> GetAvailableGroups(long deviceId, long restrictedByUserAccountId, DataParameters parameters);
		string CheckIfGroupNameExist(long groupId, string groupName);
		List<Role> GetGroupDefaultRoles(long groupId, DataParameters parameters);

		List<GroupMembershipListItem> GetUserAccountGroups(long userAccountId, long restrictedByUserAccountId,
																											 DataParameters parameters);

		long GetSupportAccountIdByGroupId(long groupId);

        void AddDeviceCategoryDevicesToGroup(long groupId, long? deviceCategoryId, long createdByUserAccountId, string deviceType);
    }
}
