using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess
{
    public class XSOARDataAccess : BaseDataAccess, IXSOARDataAccess
    {
        public string GetSalesForceAccountId(long supportAccountId)
        {
            try
            {
                const string query =
                    @"SELECT s.SalesforceAccountId FROM csp.supportaccount s WHERE s.SupportAccountId = @P_SupportAccountId;";
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@P_SupportAccountId", DbType.Int64, supportAccountId);

                    var salesForceAccountId = Connection.ExecuteScalar(command);

                    return salesForceAccountId != null ? salesForceAccountId.ToString() : string.Empty;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return string.Empty;
            }
        }
    }
}