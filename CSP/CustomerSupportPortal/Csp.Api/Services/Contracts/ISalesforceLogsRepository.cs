using System;

namespace Csp.Api.Services.Contracts {
	public interface ISalesforceLogsRepository {
		void Log(string sessionId, string salesforceUserName, string currentUser, object salesforceLogTypes, string customArguments, Exception ex);
	}

	public enum SalesforceLogTypes {
		Login,
		Logout,
		InvalidQueryFilterOperator,
        DatabaseError,
        RestApiError
	}
}
