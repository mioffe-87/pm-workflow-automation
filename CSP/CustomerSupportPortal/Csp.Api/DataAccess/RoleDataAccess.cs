using System;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Security;

namespace Csp.Api.DataAccess {

	public class RoleDataAccess : BaseDataAccess {

		/// <summary>
		/// This Method is used to get role  based on roleId
		/// </summary>
		/// <param name="roleId">roleId</param>
		/// <param name="includePrivileges"></param>
		/// <param name="includeAudit"></param>
		/// <returns>Role</returns>
		public List<Role> ReadAvailableRoles(RoleCategory roleCategory, long supportAccountId) {
			var roleList = new List<Role>();

			using (var command = Connection.GetStoredProcCommand("GetAvailableRolesByRoleCategory")) {
				Connection.AddInRoleCategory(command, roleCategory);
				Connection.AddInSupportAccountIdParameter(command, supportAccountId);

				using (IDataReader dataReader = Connection.ExecuteReader(command)) {

					while (dataReader.Read()) {

						var role = new Role();

						role.PopulateProperties(dataReader);
						roleList.Add(role);
					}
				}
			}

			return roleList;
		}

		public List<Role> ReadDefaultRoles(RoleCategory roleCategory, long supportAccountId) {
			var roleList = new List<Role>();

			using (var command = Connection.GetStoredProcCommand("GetDefaultRolesByRoleCategory")) {
				Connection.AddInRoleCategory(command, roleCategory);
				Connection.AddInSupportAccountIdParameter(command, supportAccountId);

				using (var dataReader = Connection.ExecuteReader(command)) {

					while (dataReader.Read()) {

						var role = new Role();

						role.PopulateProperties(dataReader);
						roleList.Add(role);
					}
				}
			}

			return roleList;
		}
	}
}
