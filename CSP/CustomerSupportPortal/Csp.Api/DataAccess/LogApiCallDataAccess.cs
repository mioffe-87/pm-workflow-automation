using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Csp.Api.DataAccess
{
    public class LogApiCallDataAccess : BaseDataAccess
    {
        public void LogApiErrors(string requestUrl, string method, string requestPayload, string responseStatus, string responsePayload)
        {
            string strQuery = @"INSERT INTO csp.logapicall (Application, ApiCalled, ApiMethod, RequestPayload, ResponseStatus, 
                                ResponsePayload, CreatedBy) VALUES (@p_Application, @p_ApiCalled, @p_ApiMethod, @p_RequestPayload, 
                                @p_ResponseStatus, @p_ResponsePayload, @p_CreatedBy)";
            var command = PANDBConnection.GetSqlStringCommand(strQuery);
            PANDBConnection.AddInParameter(command, "p_Application", DbType.String, "CSP");
            PANDBConnection.AddInParameter(command, "p_ApiCalled", DbType.String, requestUrl);
            PANDBConnection.AddInParameter(command, "p_ApiMethod", DbType.String, method);
            PANDBConnection.AddInParameter(command, "p_RequestPayload", DbType.String, requestPayload);
            PANDBConnection.AddInParameter(command, "p_ResponseStatus", DbType.String, responseStatus);
            PANDBConnection.AddInParameter(command, "p_ResponsePayload", DbType.String, responsePayload);
            PANDBConnection.AddInParameter(command, "p_CreatedBy", DbType.String, HttpContext.Current.User.Identity.Name);
            PANDBConnection.ExecuteNonQuery(command);
        }
    }
}
