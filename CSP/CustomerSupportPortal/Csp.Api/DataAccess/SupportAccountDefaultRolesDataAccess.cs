using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Csp.Api.DataAccess.HelperClasses;

namespace Csp.Api.DataAccess {

	public class SupportAccountDefaultRolesDataAccess : BaseDataAccess {


		#region Public Methods

		/// <summary>
		/// //The purpose of this method is to create link  for a support account : DT-0007
		/// </summary>
		/// <param name="supportAccountId">support Account Id</param>
		/// <param name="roleId">role Id</param>
		/// <param name="createdByUserAccountIdUserAccountId">created By UserAccount Id</param>
		/// <returns>void<Role></returns>
		public void Create(long supportAccountId, long roleId, long createdByUserAccountIdUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("CreateSupportAccountDefaultRole")) {

				Connection.AddInSupportAccountIdParameter(command, supportAccountId);
				Connection.AddInRoleIdParameter(command, roleId);
				Connection.AddInCreatedByUserAccountIdParameter(command, createdByUserAccountIdUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}


		/// <summary>
		/// // The purpose of this task is to read the roles for a support account: DT-0007
		/// </summary>
		/// <param name="supportAccountId">supportAccount Id</param>
		/// <param name="parameters">DataParameters</param>
		/// <returns>List<Role></returns>
		public List<Role> Read(long supportAccountId, DataParameters parameters) {

			List<Role> roleList = null;

			using (var command = Connection.GetStoredProcCommand("GetDefaultRolesBySupportAccountId")) {

				Connection.AddInSupportAccountIdParameter(command, supportAccountId);
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


		/// <summary>
		/// // The purpose of this method is to Delete link  for a support account : DT-0007
		/// </summary>
		/// <param name="supportAccountId">supportAccount Id</param>
		/// <param name="roleId">role Id</param>
		/// <param name="deletedByUserAccountId">deleted By UserAccount Id</param>
		/// <returns>void</returns>
		public void Delete(long supportAccountId, long roleId, long deletedByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("DeleteSupportAccountDefaultRole")) {

				Connection.AddInSupportAccountIdParameter(command, supportAccountId);
				Connection.AddInRoleIdParameter(command, roleId);

				Connection.ExecuteNonQuery(command);
			}
		}

		#endregion
	}
}
