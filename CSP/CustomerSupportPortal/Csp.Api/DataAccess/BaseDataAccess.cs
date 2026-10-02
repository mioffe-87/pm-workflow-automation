using Microsoft.Practices.EnterpriseLibrary.Data;
using System;

namespace Csp.Api.DataAccess {

	public abstract class BaseDataAccess {

		#region Private fields

		private Database _connection;
		private Database _panConnection;
		private Database _panLoginConnection;

		#endregion

		#region Public Properties

		public Database Connection {
			get {
				if (_connection == null) {
					_connection = DatabaseFactory.CreateDatabase("CspDatabase");
				}
				return _connection;
			}
		}

		public Database PANDBConnection {
			get {
				if (_panConnection == null) {
					_panConnection = DatabaseFactory.CreateDatabase("PANDatabase");
				}
				return _panConnection;
			}
		}

		public Database PANLoginConnection {
			get {
				if (_panLoginConnection == null) {
					_panLoginConnection = DatabaseFactory.CreateDatabase("PANLoginDatabase");
				}
				return _panLoginConnection;
			}
        }

        #endregion

        #region Common methods

        /// <summary>
        /// Get boolean result from SELECT query from Database.
        /// NOTE: SELECT must return an aggregation result, which guaranties 1 record.
        /// Query example: 
        /// <code>
        /// SELECT COUNT(1) > 0 IsOnPremController
        ///   FROM lookups l
        ///   WHERE l.LookupType = 'SW_UPDATE_ON_PREM_CONTROLLER'
        ///   AND l.LookupKey = 'CSP_ID'
        ///   AND l.LookupValue = @supportAccountId;";
        /// </code>
        /// </summary>
        /// Call example: 
        /// <code>
        /// bool isOnPremController = GetBoolanFlagFromDb(sqlQuery, "IsOnPremController", (n, c) =>
        /// {
        ///   n.AddInParameter(c, "@supportAccountId", DbType.UInt64, supportAccountId);
        /// });
        /// </code>
        /// <param name="sqlQuery">Query to run.</param>
        /// <param name="valueColumnName">Column name, which the query returns; usually COUNT.</param>
        /// <returns>Result of the SQL run.</returns>
        public bool GetBoolanFlagFromDb(string sqlQuery, string valueColumnName, Action<Database, System.Data.Common.DbCommand> fnCallBeforeExecute)
        {
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                fnCallBeforeExecute?.Invoke(Connection, command);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    dataReader.Read();
                    return Convert.ToBoolean(dataReader[valueColumnName]);
                }
            }
        }

        #endregion
    }
}
