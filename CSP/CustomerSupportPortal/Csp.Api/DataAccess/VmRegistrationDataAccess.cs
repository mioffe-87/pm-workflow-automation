using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using System.Data;
using System.Web;

namespace Csp.Api.DataAccess
{
    public class VmRegistrationDataAccess : BaseDataAccess, IVmRegistrationDataAccess
    {
        public long[] GetRegisteredDevicesWithoutSupportAccountIds(EVmType vmType)
        {
            try
            {
                const string sqlQuery = @"SELECT d.DeviceId FROM csp.device d  WHERE d.RegistrationDate IS NOT NULL AND d.SupportAccountId IS NULL AND
                                          d.CreatedOn >= '2013/07/12'";

                List<long> deviceIds = new List<long>();

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            deviceIds.Add(Convert.ToInt64(dataReader["DeviceId"]));
                        }
                        return deviceIds.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
            }
            return new long[0];
        }

        public long? GetRegisteringUserSupportAccountIfOnlyOne(EVmType vmType, long deviceId)
        {
            const long systemId = 1;
            long userAccountId;

            try
            {
                string sqlDeviceQuery = @"SELECT d.CreatedByUserAccountId FROM csp.device d WHERE d.DeviceId = @p_DeviceId;";
                using (var command = Connection.GetSqlStringCommand(sqlDeviceQuery))
                {
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                    var createdByUserAccountId = Connection.ExecuteScalar(command);
                    if (createdByUserAccountId == null || createdByUserAccountId == DBNull.Value || Convert.ToInt64(createdByUserAccountId) == systemId)
                    {
                        return null;
                    }
                    else
                    {
                        userAccountId = Convert.ToInt64(createdByUserAccountId);
                    }
                }

                string sqlMembershipQuery = @"SELECT distinct m.SupportAccountId FROM csp.Membership m JOIN csp.Membershiproles mr ON m.MembershipId = mr.MembershipId 
                                                JOIN csp.Role r ON r.RoleId = mr.RoleId WHERE m.UserAccountId = @p_UserAccountId AND m.SupportAccountId IS NOT NULL 
                                                AND r.Category IN(3, 4) LIMIT 1;";

                using (var command = Connection.GetSqlStringCommand(sqlMembershipQuery))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
                    var supportAccountId = Connection.ExecuteScalar(command);
                    if (supportAccountId == null || supportAccountId == DBNull.Value)
                    {
                        return null;
                    }
                    return Convert.ToInt64(supportAccountId);
                }
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
            }
            return null;
        }

        public void SetDeviceSupportAccountId(EVmType vmType, long deviceId, long supportAccountId)
        {
            try
            {
                string sqlMembershipQuery = @"UPDATE csp.device SET SupportAccountId = @p_SupportAccountId WHERE DeviceId = @p_DeviceId";

                using (var command = Connection.GetSqlStringCommand(sqlMembershipQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
            }
        }

        public long? GetRelatedVmSupportAccountIfExists(EVmType vmType, long deviceId)
        {
            try
            {
                const string sqlVMStub = @"SELECT distinct d.SupportAccountId FROM csp.license l JOIN csp.device d ON d.DeviceId = l.VMStubDeviceId
                                       WHERE l.VMStubDeviceId IS NOT NULL AND l.VMStubDeviceId > 0 AND l.DeviceId = @p_DeviceId limit 1;";

                using (var command = Connection.GetSqlStringCommand(sqlVMStub))
                {
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                    var supportAccountId = Connection.ExecuteScalar(command);
                    if (supportAccountId != null && supportAccountId != DBNull.Value)
                    {
                        return Convert.ToInt64(supportAccountId);
                    }
                }

                const string sqlDeviceQuery = @"SELECT d.SupportAccountId FROM csp.license l JOIN csp.device d ON d.DeviceId = l.DeviceId
                                            WHERE l.LicenseId IN (SELECT distinct VMParentLicenseId FROM csp.license WHERE VMParentLicenseId IS NOT NULL
                                            AND VMParentLicenseId > 0) LIMIT 1;";

                using (var command = Connection.GetSqlStringCommand(sqlDeviceQuery))
                {
                    var supportAccountId = Connection.ExecuteScalar(command);
                    if (supportAccountId != null && supportAccountId != DBNull.Value)
                    {
                        return Convert.ToInt64(supportAccountId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
            }
            return null;
        }
    }
}
