using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Csp.Api.DataAccess.HelperClasses;

namespace Csp.Api.DataAccess {

	public class GroupDefaultRolesDataAccess : BaseDataAccess {

		#region Public Methods

		/// <summary>
		/// This method is used for creating Defaultroles for a group
		/// </summary>
		/// <param name="groupId">Group Id</param>
		/// <param name="roleId">Role Id</param>
		/// <param name="createdByUserAccountId">Created By User Account Id</param>
		/// <returns>void</returns>
		public void Create(long groupId, long roleId, long createdByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("CreateGroupDefaultRole")) {

				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddInRoleIdParameter(command, roleId);
				Connection.AddInCreatedByUserAccountIdParameter(command, createdByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}


		/// <summary>
		/// This method is used for geting Defaultroles List based on groupID
		/// </summary>
		/// <param name="groupId">group Id</param>
		/// <param name="parameters">Pagination Parameter</param>
		/// <returns>List<Role></returns>
		public List<Role> Read(long groupId, DataParameters parameters) {

			List<Role> roleList;

			using (var command = Connection.GetStoredProcCommand("GetDefaultRolesByGroupId")) {
				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddPaginationParameters(command, parameters);

				// Geting Role List From database
				using (IDataReader dataReader = Connection.ExecuteReader(command)) {

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
