using System;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic.Salesforce {

	/// <summary>
	/// This class helps ensure that no more than 10 session requests are made
	/// and that any re-try-able errors (aka SF errors) are reported back
	/// to the session manager so it can gracefully recover the session for
	/// all other concurrent requests
	/// </summary>
	internal class RetryAbleHelper {

		private readonly ISalesforceLogsRepository _salesforceLogsRepository;

		private readonly SalesforceSessionManager _sessionMgr = SalesforceSessionManager.Instance;

		private const int MaxAttempts = 10;
		private int _attempt = 0;
		private SessionInfo _sessionInfo;

		public RetryAbleHelper(ISalesforceLogsRepository salesforceLogsRepository) {
			_salesforceLogsRepository = salesforceLogsRepository;
		}

		/// <summary>
		/// Attempts to retrieve a session from the SalesforceSessionManager
		/// and counts the attempt against the max attempts
		/// </summary>
		/// <returns></returns>
		public SessionInfo GetSession() {
			_attempt++;
			_sessionInfo = _sessionMgr.GetSession(_salesforceLogsRepository);
			return _sessionInfo;
		}

		/// <summary>
		/// Reports the SF exception (if possible, if the log-in failed, then there's nothing to report)
		/// Then returns whether the maximum retries have been exceeded.  If so you must throw the exception up the chain
		/// </summary>
		/// <returns>Returns true when the max retries are exceeded and you should "throw;" the exception</returns>
		public bool ShouldThrow(Exception ex) {

			Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

			if (_sessionInfo != null) {
				_sessionMgr.ReportFailedSession(_sessionInfo.CurrentSessionGeneration, _salesforceLogsRepository);
			}
			var exceededMaxAttempts = _attempt >= MaxAttempts;

			if (exceededMaxAttempts) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(new RetryAbleException("Salesforce request failed after exceeding maximum retry attempts", ex));
			}

			return exceededMaxAttempts;
		}
	}
}