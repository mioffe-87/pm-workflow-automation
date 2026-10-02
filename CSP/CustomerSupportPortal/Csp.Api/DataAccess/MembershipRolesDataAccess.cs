using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;
using System.Configuration;
using Newtonsoft.Json;

namespace Csp.Api.DataAccess {

	public class MembershipRolesDataAccess : BaseDataAccess {


		#region Public Methods

		/// <summary>
		///  This Method is used to join membership and role by using membershipID,roleID
		/// </summary>
		/// <param name="membershipId">Membership Id</param>
		/// <param name="roleId">roleId</param>
		/// <param name="createdByUserAccountIdUserAccountId">created By UserAccount Id </param>
		/// <returns>void</returns>
		public void Create(long membershipId, long roleId, long createdByUserAccountIdUserAccountId) 
        {
            bool addRbackAdminRole = false;
            using (var command = Connection.GetStoredProcCommand("CreateMembershipRole"))
            {
				Connection.AddInMembershipIdParameter(command, membershipId);
				Connection.AddInRoleIdParameter(command, roleId);
				Connection.AddInCreatedByUserAccountIdParameter(command, createdByUserAccountIdUserAccountId);
                Connection.AddOutParameter(command, "p_addRbackAdminRole", DbType.Boolean, 1);
                Connection.ExecuteNonQuery(command);
                addRbackAdminRole = command.Parameters["p_addRbackAdminRole"].Value != DBNull.Value
                    && Convert.ToBoolean(command.Parameters["p_addRbackAdminRole"].Value);
            }
            if (addRbackAdminRole) // CSP-1871 Create RBAC roles on the hub from CSP; call hub if SP CreateMembershipRole says so
            {
                const string selectStatement = @"SELECT m.MembershipId, u.Email, m.SupportAccountId
  FROM useraccount u 
  JOIN membership m ON u.UserAccountId = m.UserAccountId
  WHERE m.MembershipId = @p_membershipId;";
                string email = string.Empty;
                long said = 0L;
                try
                {
                    using (var command = Connection.GetSqlStringCommand(selectStatement))
                    {
                        Connection.AddInParameter(command, "@p_membershipId", DbType.Int64, membershipId);
                        using (var dataReader = Connection.ExecuteReader(command))
                        {
                            while (dataReader.Read())
                            {
                                email = dataReader["Email"].ToString();
                                said = Convert.ToInt64(dataReader["SupportAccountId"].ToString());
                            }
                        }
                    }
                    // call hub to register RBACK Super Admin role
                    new UserAccountDataAccess().RegisterAdminRbacRoleForUser(email, said);
                }
                catch (Exception wex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(wex);
                    throw;
                }
            }
		}


		/// <summary>
		/// This Method is used to get roles  based on membershipId,roleId
		/// </summary>
		/// <param name="membershipId">Membership Id</param>
		/// <param name="parameters"></param>
		/// <returns>List<Role></returns>
		public List<Role> Read(long membershipId, DataParameters parameters) {

			List<Role> roleList;

			using (var command = Connection.GetStoredProcCommand("GetMembershipRolesByMembershipId")) {

				Connection.AddInMembershipIdParameter(command, membershipId);
				Connection.AddPaginationParameters(command, parameters);

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


		/// <summary>
		/// This Method is used to  Delete role  based on membershipId,roleId
		/// </summary>
		/// <param name="membershipId"></param>
		/// <param name="roleId"></param>
		/// <param name="deletedByUserAccountId"></param>
		/// <returns>List<Role></returns>
		public void Delete(long membershipId, long roleId, long deletedByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("DeleteMembershipRole")) {

				Connection.AddInMembershipIdParameter(command, membershipId);
				Connection.AddInRoleIdParameter(command, roleId);
				Connection.AddInDeletedByUserAccountIdParameter(command, deletedByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}

		#endregion

       public List<string> GetRolesByUserAccountId(long userAccontId, long supportAccountId)
        {

            List<string> roleList;

            using (var command = Connection.GetStoredProcCommand("GetRolesByUserAccountId"))
            {

                Connection.AddInParameter(command, "p_userAccountId", DbType.Int64, userAccontId);
                Connection.AddInParameter(command, "p_supportAccountId", DbType.Int64, supportAccountId);

                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {

                    roleList = new List<string>();

                    while (dataReader.Read())
                    {
                        roleList.Add(Convert.ToString(dataReader["name"]));
                    }
                }
            }

            return roleList;
        }
	}
}
