using System;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class SalesforceLogsRepository : ISalesforceLogsRepository {

		public void Log(string sessionId, string salesforceUserName, string currentUser, object salesforceLogTypes,
										string customArguments, Exception ex) {

			using (var db = new cspEntities()) {
				var log = new salesforcelog {
					SessionId = sessionId ?? string.Empty,
					SalesforceUserName = salesforceUserName ?? string.Empty,
					CurrentUser = currentUser ?? string.Empty,
					LogType = salesforceLogTypes.ToString(),
					CustomArgs = customArguments ?? string.Empty,
					Message = string.Empty,
					StackTrace = string.Empty,
					CompleteError = string.Empty,
					Created = DateTime.Now,
				};

				if (ex != null) {
					log.Message = ex.Message;
					log.StackTrace = ex.StackTrace ?? string.Empty;
					log.CompleteError = ex.ToString();
				}

				db.salesforcelogs.Add(log);
				db.SaveChanges();
			}
		}
	}
}
