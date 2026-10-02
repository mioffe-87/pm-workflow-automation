using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using System.Data.Common;
using Csp.Api.DataAccess;
using System.Data;
using System.Transactions;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {

	public class GroupLogic : BaseLogic, IGroupLogic {

		private readonly ISupportAccountGroupRepository _supportAccountGroupRepository;

		public GroupLogic(ISupportAccountGroupRepository supportAccountGroupRepository) {
			_supportAccountGroupRepository = supportAccountGroupRepository;
		}

		/// <summary>
		/// This method is used to create Group
		/// </summary>
		/// <param name="group">Group</param>
		/// <returns>Group</returns>
		public Group CreateGroup(Group group) {

			Group resultGroup = new Group();
			GroupDataAccess groupDataAccess = new GroupDataAccess();

			using (TransactionScope transactionScope = new TransactionScope()) {

				// Creating group
				resultGroup = groupDataAccess.Create(group);


				foreach (Role role in group.DefaultRoles) {

					CreateGroupDefaultRole(resultGroup.Id, role.Id, resultGroup.CreatedByUserAccountId);
				}
				resultGroup.DefaultRoles = GetGroupDefaultRoles(resultGroup.Id, new DataParameters());
				transactionScope.Complete();
			}

			return resultGroup;
		}


		/// <summary>
		/// This method is used to get Group Details based on GroupID
		/// </summary>
		/// <param name="groupId">Group ID</param>
		/// <param name="restrictedByUserAccountId">RestrictedByUserAccount ID</param>
		/// <returns>Group</returns>
		public Group GetGroup(long groupId, long restrictedByUserAccountId) {

			Group group = new Group();
			GroupDataAccess groupDataAccess = new GroupDataAccess();

			group = groupDataAccess.Read(groupId, restrictedByUserAccountId);

			group.DefaultRoles = GetGroupDefaultRoles(groupId, new DataParameters());
			return group;
		}


		public Group UpdateGroupNameAndDescription(Group group, long restrictedByUserAccountId) {

			Group resultGroup = new Group();
			GroupDataAccess groupDataAccess = new GroupDataAccess();

			resultGroup = groupDataAccess.Update(group, restrictedByUserAccountId);

			return resultGroup;
		}


		/// <summary>
		/// This method is used to delete group based on Group ID
		/// </summary>
		/// <param name="groupId">Group ID</param>
		/// <param name="deletedByUserAccountId">Deleted By User Account Id</param>
		/// <returns>void</returns>
		public void DeleteGroup(long groupId, long deletedByUserAccountId) {

			GroupDataAccess groupDataAccess = new GroupDataAccess();

			groupDataAccess.Delete(groupId, deletedByUserAccountId);
		}


		/// <summary>
		/// This method is used to retrive list of Group based on Support Account ID
		/// </summary>
		/// <param name="supportAccountId">support Account Id</param>
		/// <param name="roleSecurities"></param>
		/// <param name="parameters">Pagination Parameter</param>
		/// <param name="restrictedByUserAccountId"></param>
		public List<GroupListItem> GetSupportAccountGroups(long supportAccountId, long restrictedByUserAccountId, List<RoleSecurity> roleSecurities, DataParameters parameters) {

			return _supportAccountGroupRepository.ReadSupportAccountGroups(supportAccountId, restrictedByUserAccountId, roleSecurities.ToArray(), parameters);
		}


		/// <summary>
		/// This method is used to retrive list of Group based on Device ID
		/// </summary>
		/// <param name="supportAccountId">support Account Id</param>
		/// <param name="parameters">Pagination Parameter</param>
		/// <returns>List<Group></returns>
		public List<GroupListItem> GetDeviceGroups(long deviceId, long restrictedByUserAccountId, DataParameters parameters) {

			GroupDataAccess groupDataAccess = new GroupDataAccess();

			return groupDataAccess.ReadDeviceGroups(deviceId, restrictedByUserAccountId, parameters);
		}


		/// <summary>
		/// This method is used for creating Defaultroles for a group
		/// </summary>
		/// <param name="groupId">Group Id</param>
		/// <param name="roleId">Role Id</param>
		/// <param name="createdByUserAccountId">Created By User Account Id</param>
		/// <returns>void</returns>
		public void CreateGroupDefaultRole(long groupId, long roleId, long createdByUserAccountId) {

			GroupDefaultRolesDataAccess groupDefaultRolesDataAccess = new GroupDefaultRolesDataAccess();

			groupDefaultRolesDataAccess.Create(groupId, roleId, createdByUserAccountId);
		}


		/// <summary>
		/// This method is used for geting Defaultroles List based on groupID
		/// </summary>
		/// <param name="groupId">group Id</param>
		/// <param name="parameters">Pagination Parameter</param>
		/// <returns>List<Role></returns>
		public List<Role> GetGroupDefaultRoles(long groupId, DataParameters parameters) {

			GroupDefaultRolesDataAccess groupDefaultRolesDataAccess = new GroupDefaultRolesDataAccess();

			return groupDefaultRolesDataAccess.Read(groupId, parameters);
		}


		/// <summary>
		/// This method is used to create relation between group and device based on paameter passed
		/// </summary>
		/// <param name="groupId">Group Id</param>
		/// <param name="deviceId">Device Id</param>
		/// <param name="createdByUserAccountId">Created By User Account Id</param>
		/// <returns>void</returns>
		public void CreateGroupDevice(long groupId, long deviceId, long createdByUserAccountId) {

			GroupDevicesDataAccess groupDevicesDataAccess = new GroupDevicesDataAccess();

			groupDevicesDataAccess.Create(groupId, deviceId, createdByUserAccountId);
		}


		/// <summary>
		/// This method is used to delete device relation with a group based on parameter passed
		/// </summary>
		/// <param name="groupId">Group Id</param>
		/// <param name="deviceId">Device Id</param>
		/// <returns>void</returns>
		public void DeleteGroupDevice(long groupId, long deviceId, long deletedByUserAccountId) {

			GroupDevicesDataAccess groupDevicesDataAccess = new GroupDevicesDataAccess();

			groupDevicesDataAccess.Delete(groupId, deviceId, deletedByUserAccountId);
		}


		/// <summary>
		/// This method is used to retrive list of Group based on User Account ID
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <param name="restrictedByUserAccountId"></param>
		/// <param name="parameters"></param>
		/// <returns> List<GroupListItem></returns>
		public List<GroupMembershipListItem> GetUserAccountGroups(long userAccountId, long restrictedByUserAccountId, DataParameters parameters) {

			GroupDataAccess groupDataAccess = new GroupDataAccess();

			return groupDataAccess.ReadUserAccountGroups(userAccountId, restrictedByUserAccountId, parameters);
		}

		public long GetSupportAccountIdByGroupId(long groupId) {
			return _supportAccountGroupRepository.GetSupportAccountIdByGroupId(groupId);
		}

	    public void AddDeviceCategoryDevicesToGroup(long groupId, long? deviceCategoryId, long createdByUserAccountId, string deviceType)
	    {
            _supportAccountGroupRepository.AddDeviceCategoryDevicesToGroup(groupId, deviceCategoryId, createdByUserAccountId, deviceType);
	    }


	    public List<GroupListItem> GetAvailableGroups(long deviceId, long restrictedByUserAccountId, DataParameters parameters) {

			GroupDataAccess groupDataAccess = new GroupDataAccess();

			return groupDataAccess.ReadAvailableGroups(deviceId, restrictedByUserAccountId, parameters);
		}

		public string CheckIfGroupNameExist(Int64 groupId, string groupName) {

			GroupDataAccess groupDataAccess = new GroupDataAccess();

			return groupDataAccess.CheckIfGroupNameExist(groupId, groupName);
		}
	}
}
