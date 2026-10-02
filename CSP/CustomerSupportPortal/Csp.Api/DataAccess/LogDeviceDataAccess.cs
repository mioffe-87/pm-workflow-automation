using System.Data;

namespace Csp.Api.DataAccess {
	public class LogDeviceDataAccess : BaseDataAccess {

		public void Log(long deviceId, string comments, long userAccountId) {

			const string sqlQuery = @"
INSERT INTO device_master_bkp 
            (
      lid,
      serialnum,
      regdate,
      acctid,
      part_id,
      ship_date,
      active,
      shipnum,
      device_name,
      comments,
      last_modified,
      src,
      dest,
      sfdcacctid,
      vsys,
      ispublic,
      `grouping`,
      pan_notes,
      nssynched,
      useCDN,
      grace,
      billToCountry,
      devCity,
      devZipCode,
      devCountry,
      IsVM,
      CpuId,
      UuId,
      MacAddress,
      reason,
      whoby,
      bkpWhen,
      devAddress1,
      devAddress2
)
SELECT 
      d.lid,
      d.serialnum,
      d.regdate,
      d.acctid,
      d.part_id,
      d.ship_date,
      d.active,
      COALESCE(d.shipnum, 0),
      d.device_name,
      d.comments,
      d.last_modified,
      d.src,
      d.dest,
      d.sfdcacctid,
      d.vsys,
      d.ispublic,
      d.grouping,
      d.pan_notes,
      d.nssynched,
      d.useCDN,
      d.grace,
      d.billToCountry,
      d.devCity,
      d.devZipCode,
      d.devCountry,
      d.IsVM,
      d.CpuId,
      d.UuId,
      d.MacAddress,
      @p_Comments,
      @p_UserAccountId,
      CURRENT_TIMESTAMP(),
      d.devAddress1,
      d.devAddress2
FROM   device_master d 
WHERE  d.lid = @p_DeviceId ";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				PANDBConnection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
				PANDBConnection.AddInParameter(command, "@p_Comments", DbType.String, comments);
				PANDBConnection.AddInParameter(command, "@p_UserAccountId", DbType.String, userAccountId.ToString()); //TODO Should we change this to the user's display name now that this is a text field?

				PANDBConnection.ExecuteNonQuery(command);
			}
		}
	}
}