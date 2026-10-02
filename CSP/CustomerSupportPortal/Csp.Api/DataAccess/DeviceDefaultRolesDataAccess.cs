using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Csp.Api.DataAccess.HelperClasses;

namespace Csp.Api.DataAccess {

	public class DeviceDefaultRolesDataAccess : BaseDataAccess {

		#region Public Methods

		/// <summary>
		/// This method is to create DefaultRole for a device
		/// </summary>
		/// <param name="deviceId">DeviceId</param>
		/// <param name="roleId">RoleId</param>
		/// <param name="createdByUserAccountId">Created By User Account Id</param>
		/// <returns>void</returns>
		public void Create(long deviceId, long roleId, long createdByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("CreateDeviceDefaultRole")) {

				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddInRoleIdParameter(command, roleId);
				Connection.AddInCreatedByUserAccountIdParameter(command, createdByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}


		/// <summary>
		///  This method is to Delete DefaultRole for a device based on Device ID and Role ID
		/// </summary>
		/// <param name="deviceId">Device ID</param>
		/// <param name="roleId">Role ID</param>
		/// <param name="deletedByUserAccountId">Deleted By User Account Id</param>
		/// <returns>void</returns>
		public void Delete(long deviceId, long roleId, long deletedByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("DeleteDeviceDefaultRole")) {

				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddInRoleIdParameter(command, roleId);
				Connection.AddInDeletedByUserAccountIdParameter(command, deletedByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}


		/// <summary>
		///  This method is to get DefaultRoles for a device based on Device ID
		/// </summary>
		/// <param name="deviceId">DeviceId</param>
		/// <param name="parameters">Pagination Parameter</param>
		/// <returns>List<Role></returns>
		public List<Role> Read(long deviceId, DataParameters parameters) {

			List<Role> roleList;

			using (var command = Connection.GetStoredProcCommand("GetDefaultRolesByDeviceId")) {

				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddPaginationParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {

					roleList = new List<Role>();

					while (dataReader.Read()) {

						var role = new Role();

						role.PopulateProperties(dataReader);
						roleList.Add(role);
					}
				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return roleList;
		}

		#endregion

	}
}
