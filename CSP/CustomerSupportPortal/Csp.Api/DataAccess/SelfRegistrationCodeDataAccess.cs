using System.Data;
using System;
using Csp.Api.Model.Enums;

namespace Csp.Api.DataAccess {

    public class SelfRegistrationCodeDataAccess : BaseDataAccess
    {

        public string GetSelfRegistrationCode(long supportAccountId)
        {
            const string sqlQuery = @"
                Select code from  csp.selfregistrationcodes src
                Where src.SupportAccountId = @p_SupportAccountId 
                    and src.RegisteredAppId = @p_RegisteredAppId
                    and src.IsActive = 1
                    and (src.ExpirationDate is null
                            || src.ExpirationDate > now() );";

            string code = String.Empty;
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int32, supportAccountId);
                    Connection.AddInParameter(command, "@p_RegisteredAppId", DbType.Int32,
                        Convert.ToInt32(RegisteredApp.CSP));

                    using (IDataReader dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            code = dataReader["code"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return code;
        }

        public void DisableRegistrationCode(long supportAccountId)
        {
            const string sqlQuery = @"
                Update csp.selfregistrationcodes src
                    Set isActive = 0
                Where src.SupportAccountId = @p_supportAccountId
                      and src.RegisteredAppId = @p_RegisteredAppId
                      and src.IsActive = 1;";

            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int32, supportAccountId);
                    Connection.AddInParameter(command, "@p_RegisteredAppId", DbType.Int32,
                        Convert.ToInt32(RegisteredApp.CSP));

                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public bool CodeExists(string newCode)
        {
            const string sqlQuery = @"
                Select Count(*) cnt from  csp.selfregistrationcodes src
                Where src.Code = @p_Code and src.RegisteredAppId = @p_RegisteredAppId ;";

            bool codeExists = false;

            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_Code", DbType.String, newCode);
                    Connection.AddInParameter(command, "@p_RegisteredAppId", DbType.Int32,
                        Convert.ToInt32(RegisteredApp.CSP));

                    using (IDataReader dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            codeExists = Convert.ToInt32(dataReader["cnt"].ToString()) > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return codeExists;
        }

        public void AddNewSelfRegistration(long supportAccountId, string newCode, Nullable<DateTime>  expirationDate,
            long createdByUserAccountId)
        {
            const string sqlQuery = @"
                Insert Into csp.selfregistrationcodes (SupportAccountId, Code, ExpirationDate, IsActive, CreatedOn, CreatedByUserAccountId, RegisteredAppId)
                Values (@p_SupportAccountId, @p_Code, @p_ExpirationDate, true, Now(), @p_CreatedByUserAccountId, @p_RegisteredAppId);";

            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int32, supportAccountId);
                    Connection.AddInParameter(command, "@p_Code", DbType.String, newCode);
                    Connection.AddInParameter(command, "@p_ExpirationDate", DbType.DateTime, expirationDate);
                    Connection.AddInParameter(command, "@p_CreatedByUserAccountId", DbType.Int32,
                        createdByUserAccountId);
                    Connection.AddInParameter(command, "@p_RegisteredAppId", DbType.Int32,
                        Convert.ToInt32(RegisteredApp.CSP));

                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }
    }
}
