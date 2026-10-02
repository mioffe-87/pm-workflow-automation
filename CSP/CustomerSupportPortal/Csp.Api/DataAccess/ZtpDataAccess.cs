using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.HelperClasses;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Csp.Api.DataAccess
{
    public class ZtpDataAccess : BaseDataAccess, IZtpDataAccess
    {
        /// <summary>
        /// Get Panorama ZTP Tenant
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="serialNumber"></param>
        /// <param name="appPortalApp"></param>
        /// <returns></returns>
        public long? GetPanoramaZtpTenant(long supportAccountId, AppPortalAppType appPortalApp)
        {
            long? tenantId = null;
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetPanoramaZtpTenant"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountID", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "p_AppType", DbType.String, appPortalApp.ToDescription());

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            tenantId = Convert.ToInt64(dataReader["TenantId"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return tenantId;
        }

        /// <summary>
        /// Generates tenant id for any cloud service
        /// </summary>
        /// <param name="retryLimit"></param>
        /// <param name="isLoggingService"></param>
        /// <returns></returns>
        public long GenerateTenantId(int retryLimit, bool isLoggingService = false)
        {
            long tenantId = 0;
            try
            {
                using (var command = Connection.GetStoredProcCommand("GenerateTenantId"))
                {
                    Connection.AddInParameter(command, "p_RetryLimit", DbType.Int32, retryLimit);
                    Connection.AddInParameter(command, "p_IsLGS", DbType.Boolean, isLoggingService);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            tenantId = Convert.ToInt64(dataReader["newTenantId"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return tenantId;
        }

        /// <summary>
        /// Creates cloud service tenant for Primary and Secondary Panorama
        /// </summary>
        /// <param name="requestData"></param>
        /// <param name="tenantId"></param>
        /// <param name="appInternalName"></param>
        public bool AddPanoramaAssociation(PanoramaAssociationRequest requestData, long? tenantId, string appInternalName)
        {
            int result = 0;

            try
            {
                using (var command = Connection.GetStoredProcCommand("CreateZtpTenantForPanorama"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, requestData.SupportAccountId);
                    Connection.AddInParameter(command, "p_TenantId", DbType.Int64, tenantId);
                    Connection.AddInParameter(command, "p_PanoramaSerialNumber", DbType.String, requestData.PrimarySerialNumber);
                    Connection.AddInParameter(command, "p_SecondarySerialNumber", DbType.String, requestData.SecondarySerialNumber);
                    Connection.AddInParameter(command, "p_AppInternalName", DbType.String, appInternalName);
                    Connection.AddInParameter(command, "p_UserEmail", DbType.String, requestData.UserEmail);
                    result = (int)Connection.ExecuteScalar(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return result > 0 ? true : false;
        }

        /// <summary>
        /// update panorama association(s)
        /// </summary>
        /// <param name="requestData"></param>
        /// <param name="tenantId"></param>
        /// <param name="appInternalName"></param>
        /// <returns></returns>
        public bool UpdatePanoramaAssociation(PanoramaAssociationRequest requestData, long? tenantId, string appInternalName)
        {
            int result = 0;
            const string sqlQuery = @"UPDATE csp.secaaspanoramaregistration s JOIN tenantlookup t ON s.TenantId = t.TenantId 
                                     JOIN appportalapps a ON t.AppPortalAppsId = a.AppPortalAppsId 
                                     SET PanoramaSerialNumber = @p_PanoramaSerialNumber, SecondarySerialNumber = @p_SecondarySerialNumber,
                                     s.UpdatedBy = @p_UpdatedBy, s.UpdatedDate = @p_UpdatedDate
                                     WHERE t.SupportAccountId = @p_SupportAccountId AND t.TenantId = @p_TenantId  AND a.AppInternalName = @p_AppInternalName AND s.IsActive = 1;";
            try
            {
                if (BackupPanoramaAssociation(requestData.SupportAccountId, tenantId, requestData.UserEmail, appInternalName))
                {
                    using (var command = Connection.GetSqlStringCommand(sqlQuery))
                    {
                        Connection.AddInParameter(command, "@p_PanoramaSerialNumber", DbType.String, requestData.PrimarySerialNumber);
                        Connection.AddInParameter(command, "@p_SecondarySerialNumber", DbType.String, requestData.SecondarySerialNumber);
                        Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, requestData.SupportAccountId);
                        Connection.AddInParameter(command, "@p_TenantId", DbType.Int64, tenantId);
                        Connection.AddInParameter(command, "@p_AppInternalName", DbType.String, appInternalName);
                        Connection.AddInParameter(command, "@p_UpdatedBy", DbType.String, requestData.UserEmail);
                        Connection.AddInParameter(command, "@p_UpdatedDate", DbType.DateTime, DateTime.Now);
                        result = Connection.ExecuteNonQuery(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return result > 0 ? true : false;
        }

        /// <summary>
        /// load panorama device(s)
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public List<NameValueOnly> GetDevicesForZtp(long supportAccountId)
        {

            var panoramaSerialNumbers = new List<NameValueOnly>();
            using (var command = Connection.GetStoredProcCommand("GetDevicesForZTP"))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, string.Empty);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var serialnum = new NameValueOnly
                        {
                            Name = Convert.ToString(dataReader["SerialNumber"]),
                            Value = Convert.ToString(dataReader["SerialNumber"])
                        };
                        panoramaSerialNumbers.Add(serialnum);
                    }
                }
            }
            return panoramaSerialNumbers;
        }

        /// <summary>
        /// load panorama associations
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="appInternalName"></param>
        /// <returns></returns>
        public IEnumerable<PanoramaAssociationResult> GetPanoramaAssociations(long supportAccountId, string appInternalName)
        {
            var panoramaAssociation = new List<PanoramaAssociationResult>();
            using (var command = Connection.GetStoredProcCommand("GetPanoramaAssociations"))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInParameter(command, "@p_AppInternalName", DbType.String, appInternalName);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var association = new PanoramaAssociationResult();
                        association.SerialNumber = HttpUtility.HtmlEncode(dataReader["SerialNumber"].ToString());
                        association.DeviceModel = HttpUtility.HtmlEncode(dataReader["DeviceModel"].ToString());
                        association.DeviceName = HttpUtility.HtmlEncode(dataReader["DeviceName"].ToString());
                        association.Requestor = HttpUtility.HtmlEncode(dataReader["Requestor"].ToString().Substring(0, dataReader["Requestor"].ToString().IndexOf('@')));
                        association.Requested = dataReader["Requested"] != DBNull.Value ? HttpUtility.HtmlEncode(DateTime.Parse(dataReader["Requested"].ToString()).ToString()) : String.Empty;
                        panoramaAssociation.Add(association);
                    }
                }
            }
            return panoramaAssociation;
        }

        /// <summary>
        /// load panorama association history
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public IEnumerable<PanoramaAssociationHistory> GetPanoramaAssociationHistory(long supportAccountId, string appInternalName)
        {
            var associationHistory = new List<PanoramaAssociationHistory>();

            using (var command = Connection.GetStoredProcCommand("GetZtpPanoramaAssociationHistory"))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_AppInternalName", DbType.String, appInternalName);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var history = new PanoramaAssociationHistory();
                        history.PrimarySerialNumber = HttpUtility.HtmlEncode(dataReader["PrimarySerialNumber"].ToString());
                        history.SecondarySerialNumber = HttpUtility.HtmlEncode(dataReader["SecondarySerialNumber"].ToString());
                        history.DeviceModel = HttpUtility.HtmlEncode(dataReader["DeviceModel"].ToString());
                        history.DeviceName = HttpUtility.HtmlEncode(dataReader["DeviceName"].ToString());
                        history.Status = HttpUtility.HtmlEncode(dataReader["Status"].ToString());
                        history.Requestor = dataReader["Requestor"].ToString().Contains('@') ?
                            HttpUtility.HtmlEncode(dataReader["Requestor"].ToString().Substring(0, dataReader["Requestor"].ToString().IndexOf('@')))
                                            : HttpUtility.HtmlEncode(dataReader["Requestor"].ToString());
                        history.Requested = dataReader["Requested"] != DBNull.Value ? HttpUtility.HtmlEncode(DateTime.Parse(dataReader["Requested"].ToString()).ToString()) : String.Empty;
                        associationHistory.Add(history);
                    }
                }
            }
            return associationHistory;
        }

        public bool IsPanormaDeviceActive(long supportAccountId, string serialNumber)
        {
            var result = string.Empty;
            using (var command = Connection.GetStoredProcCommand("GetDevicesForZTP"))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                result = (string) Connection.ExecuteScalar(command);
            }
            return string.IsNullOrEmpty(result) ? false : true;
        }

        private bool BackupPanoramaAssociation(long supportAccountId, long? tenantId, string userEmail, string appInternalName)
        {
            var result = 0;
            string userName = userEmail.Substring(0, userEmail.IndexOf('@'));
            const string sqlQuery = @"INSERT INTO csp.secaaspanoramaregistration_bkp (PanoramaSerialNumber, SupportAccountId, RegistrationNumber, CreatedDate, TenantId, 
                                      SecondarySerialNumber, IsHA, IsActive, UpdatedDate, UpdatedBy, IsPurging, bkp_By, bkp_Reason)
                                      SELECT s.PanoramaSerialNumber, s.SupportAccountId, s.RegistrationNumber, s.CreatedDate, s.TenantId, s.SecondarySerialNumber,
                                      s.IsHA, s.IsActive, s.UpdatedDate, s.UpdatedBy, s.IsPurging, @p_Bywho, @p_Reason FROM csp.secaaspanoramaregistration s 
                                      JOIN tenantlookup t  ON s.TenantId = t.TenantId JOIN appportalapps a ON t.AppPortalAppsId = a.AppPortalAppsId
                                      WHERE s.TenantId = @p_TenantId  AND s.SupportAccountId = @p_SupportAccountId  AND s.IsActive AND a.AppInternalName = @p_AppType;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_TenantId", DbType.Int64, tenantId);
                Connection.AddInParameter(command, "@p_Bywho", DbType.String, userName);
                Connection.AddInParameter(command, "@p_Reason", DbType.String, "Panorama Association(s) updated,");
                Connection.AddInParameter(command, "@p_AppType", DbType.String, appInternalName);
                result = Connection.ExecuteNonQuery(command);
            }

            return result > 0 ? true : false;
        }
    }
}
