using System.Data;

namespace Csp.Api.DataAccess {
	public class LogTransactionsDataAccess : BaseDataAccess {

		public void Log(string action, string srcSerialNumber, string destSerialNumber, string ipAddress, long userAccountId, string notes) {

			const string sqlQuery = @"
			INSERT INTO logtransaction 
			            (Action,
									 SourceSerialNumber, 
			             DestinationSerialNumber, 
			             IpAddress, 
			             CreatedOn, 
			             CreatedByUserAccountId, 
			             Notes) 
			VALUES      (@p_Action, 
			             @p_SrcSerialNumber, 
			             @p_DestSerialNumber, 
			             @p_IpAddress, 
									 CURRENT_TIMESTAMP, 
			             @p_UserAccountId, 
			             @p_Notes); ";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_Action", DbType.String, action);
				Connection.AddInParameter(command, "@p_SrcSerialNumber", DbType.String, srcSerialNumber);
				Connection.AddInParameter(command, "@p_DestSerialNumber", DbType.String, destSerialNumber);
				Connection.AddInParameter(command, "@p_IpAddress", DbType.String, ipAddress);
				Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
				Connection.AddInParameter(command, "@p_Notes", DbType.String, notes);

				Connection.ExecuteNonQuery(command);
			}
		}
	}
}