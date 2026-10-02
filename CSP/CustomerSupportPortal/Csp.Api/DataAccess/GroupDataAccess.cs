using Csp.Api.Model;
using System;
using System.Collections.Generic;
using Csp.Api.DataAccess.HelperClasses;
using System.Data;
using MySql.Data.MySqlClient;
using Csp.Api.Exceptions;

namespace Csp.Api.DataAccess {

	public class GroupDataAccess : BaseDataAccess {
		/// <summary>
		/// This method is used to create Group
		/// </summary>
		/// <param name="group">Group</param>
		/// <returns>Group</returns>
		public Group Create(Group group) {

			var resultGroup = new Group();
			using (var command = Connection.GetStoredProcCommand("CreateGroup")) {
				Connection.AddInModelParameters(command, group, CreateOrUpdate.Create);
				using (var dataReader = Connection.ExecuteReader(command)) {

					if (dataReader.Read()) {
						resultGroup.PopulateProperties(dataReader);
					}
				}
			}

			return resultGroup;
		}


		/// <summary>
		/// This method is used to get Group Details based on GroupID
		/// </summary>
		/// <param name="groupId">Group ID</param>
		/// <param name="restrictedByUserAccountId">RestrictedByUserAccount ID</param>
		/// <returns>Group</returns>
		public Group Read(long groupId, long restrictedByUserAccountId) {

			var group = new Group();
			using (var command = Connection.GetStoredProcCommand("GetGroupByGroupId")) {
				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);

				using (var dataReader = Connection.ExecuteReader(command)) {

					if (dataReader.Read()) {
						group.PopulateProperties(dataReader);
					}

					return group;
				}
			}
		}


		/// <summary>
		/// This method is used to update group Details
		/// </summary>
		/// <param name="group">Group</param>
		///  <param name="restrictedByUserAccountId">RestrictedByUserAccount ID</param>
		/// <returns>Group</returns>
		public Group Update(Group group, long restrictedByUserAccountId) {

			var resultGroup = new Group();
			using (var command = Connection.GetStoredProcCommand("UpdateGroup")) {
				Connection.AddInModelParameters(command, group, CreateOrUpdate.Update);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				try {

					using (var dataReader = Connection.ExecuteReader(command)) {

						if (dataReader.Read()) {
							resultGroup.PopulateProperties(dataReader);
						}

					}

				} catch (MySqlException exception) {
					if (exception.Message.Contains("Concurrent update error")) {

						throw new CspNotificationException(
							"A user has already updated this Group. Please refresh your data, review the changes made and try again");
					} else {
						throw;
					}

				}
				return resultGroup;
			}
		}


		/// <summary>
		/// This method is used to delete group based on Group ID
		/// </summary>
		/// <param name="groupId">Group ID</param>
		/// <param name="deletedByUserAccountId">Deleted By User Account Id</param>
		/// <returns>void</returns>
		public void Delete(long groupId, long deletedByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("DeleteGroup")) {
				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddInDeletedByUserAccountIdParameter(command, deletedByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}


		/// <summary>
		/// This method is used to retrive list of Group based on Support Account ID
		/// </summary>
		/// <param name="restrictedByUserAccountId"></param>
		/// <param name="parameters">Pagination Parameter</param>
		/// <param name="deviceId"></param>
		/// <returns>List<Group/></returns>
		public List<GroupListItem> ReadDeviceGroups(long deviceId, long restrictedByUserAccountId, DataParameters parameters) {


			var groupListItems = new List<GroupListItem>();

			using (var command = Connection.GetStoredProcCommand("GetGroupsByDeviceId")) {
				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {


					while (dataReader.Read()) {

						var groupListItem = new GroupListItem();
						groupListItem.PopulateProperties(dataReader);
						groupListItems.Add(groupListItem);

					}

				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return groupListItems;
		}

		/// <summary>
		/// This method is used to retrive list of Group based on support Account 
		/// </summary>
		/// <param name="userAccountId"></param>
		/// <param name="restrictedByUserAccountId"></param>
		/// <param name="parameters"></param>
		/// <returns>List<GroupListItem/></returns>
		public List<GroupMembershipListItem> ReadUserAccountGroups(long userAccountId, long restrictedByUserAccountId, DataParameters parameters) {


			var groupMembershipListItems = new List<GroupMembershipListItem>();

			using (var command = Connection.GetStoredProcCommand("GetGroupMembershipsByUserAccountId")) {
				Connection.AddInUserAccountIdParameter(command, userAccountId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {


					while (dataReader.Read()) {

						var groupMembershipListItem = new GroupMembershipListItem();
						groupMembershipListItem.PopulateProperties(dataReader);
						groupMembershipListItems.Add(groupMembershipListItem);

					}

				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return groupMembershipListItems;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="restrictedByUserAccountId"></param>
		/// <param name="parameters"></param>
		/// <returns>List<DeviceListItem></DeviceListItem></returns>
		public List<GroupListItem> ReadAvailableGroups(long deviceId, long restrictedByUserAccountId, DataParameters parameters) {

			var groupListItems = new List<GroupListItem>();

			using (var command = Connection.GetStoredProcCommand("GetAvailableGroupsBySupportAccountID")) {
				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);

				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {


					while (dataReader.Read()) {

						var groupListItem = new GroupListItem();
						groupListItem.PopulateProperties(dataReader);
						groupListItems.Add(groupListItem);

					}

				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}
			return groupListItems;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="groupName"></param>
		/// <returns></returns>
		public string CheckIfGroupNameExist(Int64 groupId, string groupName) {
			var result = "";

			using (var command = Connection.GetStoredProcCommand("CheckIfGroupNameExist")) {
				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddInParameter(command, "p_Name", DbType.String, groupName);
				Connection.AddOutParameter(command, "p_result", DbType.String, 50);
				Connection.ExecuteNonQuery(command);

				if (command.Parameters["p_result"].Value != DBNull.Value) {
					result = command.Parameters["p_result"].Value.ToString();
				}
			}

			return result;
		}
	}
}
