using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services;
using Csp.Api.Services.Contracts;

namespace Csp.Api.DataAccess
{
    public class ConsentRequestDataAccess :BaseDataAccess, IConsentRequestDataAccess
    {
        public void AddUpdateConsentRequest(string consentRequestId, long sourceAccountId, long targetAccountId, bool isActive)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("AddUpdateConsentRequest"))
                {
                    Connection.AddInParameter(command, "@p_ConsentRequestId", DbType.String, consentRequestId);
                    Connection.AddInParameter(command, "@p_IsActive", DbType.Byte, isActive);
                    Connection.AddInParameter(command, "@p_SourceAccountId", DbType.Int64, sourceAccountId);
                    Connection.AddInParameter(command, "@p_TargetAccountId", DbType.Int64, targetAccountId);

                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message.ToString());
            }
        }
    }
}
