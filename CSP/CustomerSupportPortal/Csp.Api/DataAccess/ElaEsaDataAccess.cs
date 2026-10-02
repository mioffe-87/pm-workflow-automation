using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model.ElaEsa;
using Csp.Api.Model.Wildfire;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess
{
    public class ElaEsaDataAccess : BaseDataAccess, IElaEsaDataAccess
    {
        public bool ValidateElaDeviceForRegistration(string serialNumber)
        {
            string query = @"
SELECT count(1) > 0 IsValid
    FROM csp.easkus e
    JOIN csp.eatype t on e.eatype = t.id
    JOIN partmaster p ON e.partnumber = p.partnumber
    join device d on p.partmasterid = d.partmasterid
    WHERE d.serialnumber = @p_SerialNumber
;";

            try
            {
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            return dataReader.GetColumnValue<bool>("IsValid");
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public IEnumerable<string> GetElaDeviceForDecom(long supportAccountId)
        {
            string query = @"
SELECT d.serialnumber
    FROM csp.easkus e
    JOIN csp.eatype t on e.eatype = t.id
    JOIN partmaster p ON e.partnumber = p.partnumber
    join device d on p.partmasterid = d.partmasterid AND d.supportaccountid = @p_SupportAccountId AND d.isactive = 1 AND d.deviceid IN
    (
        SELECT l2.deviceid
            FROM license l2
            JOIN partmaster p2 on l2.partmasterid = p2.partmasterid AND p2.partnumber LIKE '%-ENT%'
            AND l2.supportaccountid = @p_SupportAccountId
    )
    WHERE t.Id IN (3, 6, 9);
;";

            try
            {
                List<string> sns = new List<string>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            sns.Add(dataReader.GetColumnValue<string>("serialnumber"));
                        }
                    }
                }

                return sns;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public IEnumerable<ElaEsaDecomissionDevice> GetDecommissionPayloadBySessionId(string sessionId)
        {
            string query = @"
SELECT e.Request
    FROM csp.elaactionhistory e
    WHERE e.SessionId = @sessionId;
;";

            try
            {
                List<ElaEsaDecomissionDevice> decomDevices = new List<ElaEsaDecomissionDevice>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@sessionId", DbType.String, sessionId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            var request = dataReader.GetString(0);
                            decomDevices.AddRange(JsonConvert.DeserializeObject<DecomissionDeviceSAPRequest>(request.Trim()).Devices);
                        }
                    }
                }

                return decomDevices;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public IEnumerable<ElaEsaCapInfo> ValidateMasterAuthCodes(long supportAccountId, IEnumerable<ElaEsaCapInfo> contracts)
        {
            if (!(contracts?.Any() ?? false)) // do not validate empty collection.
            {
                return new ElaEsaCapInfo[0];
            }

            string query = @"
SELECT l.authorizationcode
    FROM license l
    JOIN partmaster p ON l.partmasterid = p.partmasterid AND p.isactive = 1
    JOIN partsubtype s ON p.partsubtypeid = s.partsubtypeid AND s.isactive = 1
    WHERE (s.pandbidentification LIKE '%SITE%ENT%'
    OR s.pandbidentification LIKE '%SITE%ESA%')
--    AND l.isactive = 1
--    AND l.expirationdate > now()
    AND l.authorizationcode in ({0});
;".Replace("{0}", string.Join(", ", contracts.Select(i => string.Format("'{0}'", i.MasterAuthCode)).ToArray()));

            try
            {
                List<string> callidusAuthCodes = new List<string>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            callidusAuthCodes.Add(dataReader.GetColumnValue<string>("authorizationcode"));
                        }
                    }
                }

                return contracts.Where(i => callidusAuthCodes.Any(v => v == i.MasterAuthCode)).ToArray();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public ElaCapsPagedResponse<IEnumerable<ElaEsaDecomedDevice>> GetElaDecomHistory(ElaEsaDecomRequest request)
        {
            string sortClause = string.Empty;
            string limitClause = string.Empty;
            string BODY_QUERY = @"
DROP TEMPORARY TABLE IF EXISTS TempDecomedDevices_{0};

CREATE TEMPORARY TABLE TempDecomedDevices_{0}
(
    ElaActionId BIGINT
) ENGINE MEMORY;

INSERT INTO TempDecomedDevices_{0}
    SELECT h.ElaActionId
        FROM
        (
            SELECT h2.EntityID, max(h2.SessionId) SessionId
                FROM csp.ElaActionHistory h2
                WHERE h2.SupportAccountId = @p_SupportAccountId
                GROUP BY h2.EntityID
        ) a
        JOIN csp.ElaActionHistory h ON a.EntityID = h.EntityID AND a.Sessionid = h.SessionId;

SELECT COUNT(1) Total
    FROM TempDecomedDevices_{0} t;

SELECT h.sessionid, h.lastupdateddate, h.entityid SerialNumber,
    COALESCE(p.partnumber, db.part_id) Sku,
    COALESCE(p.productfamily, pd.productfamily) Model,
    COALESCE(d.devicename, db.device_name) AssetName,
    h.requestdetails, h.Request OriginalRequest, h.status, h.Message, CASE WHEN u.useraccountid IS NOT NULL THEN concat(u.firstname, ' ', u.lastname) ELSE '' END User
    FROM TempDecomedDevices_{0} t
    JOIN csp.ElaActionHistory h ON t.ElaActionId = h.ElaActionId
    LEFT JOIN device d ON h.entityid = d.serialnumber
    LEFT JOIN pandb.device_master_bkp db on h.entityid = db.serialnum 
    LEFT JOIN partmaster p ON d.partmasterid = p.partmasterid
    LEFT JOIN partmaster pd ON db.part_id = pd.partnumber
    JOIN useraccount u ON u.useraccountid = h.createdbyuseraccountid
    GROUP BY h.ElaActionId
    {1}
    {2}
;

DROP TEMPORARY TABLE IF EXISTS TempDecomedDevices_{0};
";
            if (!string.IsNullOrEmpty(request.Data.SortField)
                && !string.IsNullOrEmpty(request.Data.SortOrder)
                && SortHelper.ValidateSorting(request.Data.SortField, request.Data.SortOrder))
            {
                sortClause = string.Format("ORDER BY {0} {1}", request.Data.SortField, request.Data.SortOrder);
            }
            else
            {
                sortClause = "ORDER BY LastupdatedDate DESC, SerialNumber";
            }
            limitClause = string.Format("LIMIT {0}, {1}", (request.Data.CurrentPage - 1) * request.Data.PageSize, request.Data.PageSize);
            try
            {
                // get all sessions
                List<ElaEsaDecomedDevice> devices = new List<ElaEsaDecomedDevice>();
                int count = 0;
                int total = 0;
                string sqlQuery = string.Format(BODY_QUERY, DateTime.Now.Ticks, sortClause, limitClause);
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.UInt64, request.Params.SupportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            total = dataReader.GetColumnValue<int>("Total");
                        }

                        dataReader.NextResult();
                        while (dataReader.Read())
                        {
                            IEnumerable<ElaEsaDecomissionDeviceDetail> elaEsas = null;
                            dynamic originalPayload = null;
                            string requestdetails = dataReader.GetColumnValue<string>("requestdetails");
                            string originalRequest = dataReader.GetColumnValue<string>("OriginalRequest");
                            if (!string.IsNullOrWhiteSpace(requestdetails))
                            {
                                try
                                {
                                    elaEsas = JsonConvert.DeserializeObject<ElaEsaDecomissionDeviceDetail[]>(requestdetails.Trim());
                                }
                                catch
                                {
                                    // do nothing
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(originalRequest))
                            {
                                try
                                {
                                    originalPayload = JsonConvert.DeserializeObject(originalRequest.Trim());
                                }
                                catch
                                {
                                    // do nothing
                                }
                            }

                            devices.Add(new ElaEsaDecomedDevice
                            {
                                DeviceId = ++count,
                                DecomDate = dataReader.GetColumnValue<DateTime?>("lastupdateddate"),
                                SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                Sku = dataReader.GetColumnValue<string>("Sku"),
                                Model = dataReader.GetColumnValue<string>("Model"),
                                AssetName = dataReader.GetColumnValue<string>("AssetName"),
                                ElaAuthCodes = elaEsas?.Where(i => i.AgreementType == EElaEsaAgreementType.Ela).Select(i => i.AuthCode).ToArray(),
                                ElaListPrices = elaEsas?.Where(i => i.AgreementType == EElaEsaAgreementType.Ela).Select(i => i.Price).ToArray(),
                                EsaAuthCodes = elaEsas?.Where(i => i.AgreementType == EElaEsaAgreementType.Esa).Select(i => i.AuthCode).ToArray(),
                                EsaListPrices = elaEsas?.Where(i => i.AgreementType == EElaEsaAgreementType.Esa).Select(i => i.Price).ToArray(),                                
                                isRecycle = (originalPayload?.OriginalPayload?.Count > 0 ?? false)  ? (originalPayload?.OriginalPayload[0]?.deviceReturn ?? false) : false,
                                Status = (EElaEsaActionStatus)dataReader.GetColumnValue<short>("status"),
                                Message = dataReader.GetColumnValue<string>("Message"),
                                User = dataReader.GetColumnValue<string>("User")
                            });
                        }
                    }
                }

                return new ElaCapsPagedResponse<IEnumerable<ElaEsaDecomedDevice>>
                {
                    Data = devices.ToArray(),
                    PerPage = request.Data.PageSize,
                    PageNumber = request.Data.CurrentPage,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public void ElaDecomDevicesComplete(ElaEsaDecomissionDeviceCompleteRequest request, string log)
        {
            const string updateQuerySessionError = "UPDATE csp.ElaActionHistory e SET e.status = 4, e.Message = '{1}', ResponseDetails = '{2}', LastUpdatedDate = NOW() WHERE e.sessionid = '{0}';\r\n";
            const string updateQuerySuccess = "UPDATE csp.ElaActionHistory e SET e.status = 2, ResponseDetails = '{2}', LastUpdatedDate = NOW() WHERE e.sessionid = '{0}' AND e.EntityId = '{1}';\r\n";
            const string updateQueryError = "UPDATE csp.ElaActionHistory e SET e.status = 4, e.Message = '{2}', ResponseDetails = '{3}', LastUpdatedDate = NOW() WHERE e.sessionid = '{0}' AND e.EntityId = '{1}';\r\n";

            StringBuilder sb = new StringBuilder();
            if (request.Status?.ToLower().Trim() == "success")
            {
                request.Devices.ToList().ForEach(d =>
                {
                    if (d.Status?.ToLower().Trim() == "success")
                    {
                        sb.AppendFormat(updateQuerySuccess, request.SessionId, d.SerialNumber, log);
                    }
                    else
                    {
                        sb.AppendFormat(updateQueryError, request.SessionId, d.SerialNumber, d.Message, log);
                    }
                });
            }
            else
            {
                sb.AppendFormat(updateQuerySessionError, request.SessionId, request.Message, log);
            }
            using (var command = Connection.GetSqlStringCommand(sb.ToString()))
            {
                Connection.ExecuteNonQuery(command);
            }
        }

        public void PrepDecommissionDevices(long supportAccountId, ref IEnumerable<ElaEsaDecomissionDevice> decomDevices, out long sessionId)
        {
            string query = string.Format(@"
SELECT d.serialnumber, ps.partnumber
    FROM device d
    JOIN partmaster p ON d.partmasterid = p.partmasterid
    JOIN partmaster ps ON d.partmasteridasshipped = ps.partmasterid
    WHERE d.serialnumber IN ('{0}')
    AND d.supportaccountid = @p_SupportAccountId 
    AND d.isactive = 1;
SELECT MAX(e.sessionid) LastSessionId
    FROM csp.ElaActionHistory e
    WHERE e.ActionType = 'ELADECOMDEVICES';
;", string.Join("','", decomDevices.Select(i => i.SerialNumber).ToArray()));
            sessionId = 0;
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        string sn = dataReader.GetColumnValue<string>("serialnumber");
                        decomDevices.Single(i => i.SerialNumber == sn).Material = dataReader.GetColumnValue<string>("partnumber");
                    }
                    dataReader.NextResult();
                    if (dataReader.Read())
                    {
                        sessionId = dataReader.GetColumnValue<long>("LastSessionId") + 1;
                    }
                }
            }
        }

        public void PostDecommissionDevices(long supportAccountId, long userAccountId, IEnumerable<ElaEsaDecomissionDevice> decomDevices, long sessionId, EElaEsaActionStatus statusId, string requestProps, string responseProps = null)
        {
            const string queryValue = "    ({0}, {1}, 'ELADECOMDEVICES', '{2}', {3}, {4}, {5}, '{6}', '{7}'){8}\r\n";
            int count = 1;
            int size = decomDevices.Count();
            StringBuilder sb = new StringBuilder("INSERT INTO ElaActionHistory (SupportAccountId, SessionId, ActionType, EntityID, RequestDetails, Status, CreatedByUserAccountId, Request, Response)\r\n  VALUES\r\n");
            decomDevices.ToList().ForEach(d => sb.AppendFormat(queryValue, supportAccountId, sessionId, d.SerialNumber, 
                d.ElaEsas != null ? "'" + JsonConvert.SerializeObject(d.ElaEsas) + "'" : "NULL", (int)statusId, userAccountId, requestProps, responseProps, count++ == size ? ";" : ","));
            using (var command = Connection.GetSqlStringCommand(sb.ToString()))
            {
                int affectedRecords = Connection.ExecuteNonQuery(command);
                if (affectedRecords != decomDevices.Count())
                {
                    throw new ApplicationException(string.Format("ELA/ESA Post decom session '{0}' was {1} updated.", sessionId, affectedRecords == 0 ? "not" : string.Format("partially ({0}/{1})", affectedRecords, decomDevices.Count())));
                }
            }
        }

        public Dictionary<long, string> GetElaAssociatedSupportAccounts(long supportAccountId)
        {
            const string query = @"
SELECT a.supportaccountid, a.name
    FROM csp.supportaccount s
    JOIN csp.supportaccount a on s.salesforceaccountid = a.salesforceaccountid
    WHERE s.supportaccountid = @p_SupportAccountId;
;";
            Dictionary<long, string> result = new Dictionary<long, string>();
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        result.Add(dataReader.GetColumnValue<long>("supportaccountid"), dataReader.GetColumnValue<string>("name"));
                    }
                }
            }

            return result;
        }
    }
}
