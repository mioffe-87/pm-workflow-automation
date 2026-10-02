using System;
using Csp.Api.HelperClasses;
using Csp.Api.SalesforceWebApi;
using Csp.Api.Services.Contracts;
using System.Net;

namespace Csp.Api.Logic.Salesforce {

	/// <summary>
	/// The Salesforce Session Manager shares the SF session
	/// among concurrent requests and also handles terminating and
	/// re-starting sessoins in a thread safe way to allow for graceful
	/// recovery from errors that would affect simultaneous requests
	/// 
	/// For further notes see MSDN Singleton patterns
	/// https://msdn.microsoft.com/en-us/library/ff650316.aspx
	/// </summary>
	public sealed class SalesforceSessionManager : IDisposable {

		private readonly SessionInfo _session = new SessionInfo();
		private long _maxSessionGeneration = 0;

		#region Singleton Logic
		private SalesforceSessionManager() {
			//Do nothing (private constructor so it can ONLY be accessed as a singleton)
		}

		private static readonly SalesforceSessionManager instance = new SalesforceSessionManager();
		public static SalesforceSessionManager Instance {
			get {
				return instance;
			}
		}
		#endregion

		/// <summary>
		/// Get session will return the current active session
		/// and if there is no active session will create one.
		/// 
		/// The entire method logic must be in the lock to 
		/// prevent reading of the session by another thread
		/// while it is still in the process of being created
		/// </summary>
		/// <returns></returns>
		public SessionInfo GetSession(ISalesforceLogsRepository salesforceLogsRepository) {

			lock (_session) {

				if (_session.IsActive() == false) {
					Login(salesforceLogsRepository);
				}

				return _session;
			}
		}

		/// <summary>
		/// Reporting failures will alert the session manager to
		/// try logging out and will mark this session as inactive
		/// so that it can be re-created the next time GetSession()
		/// is called.
		/// 
		/// The entire method logic must be in the lock to
		/// prevent multiple calls from simultaneously crashed 
		/// threads from reporting their error at the same time.
		/// Under this model, only the first report will lead to
		/// a logout. The rest will do nothing. This way we only
		/// logout and then login once per error, instead of one
		/// per error per thread.
		/// </summary>
		/// <param name="failedSessionGeneration"></param>
		public void ReportFailedSession(long failedSessionGeneration, ISalesforceLogsRepository salesforceLogsRepository) {

			lock (_session) {
				var sessionAlreadyInactive = _session.CurrentSessionGeneration == SessionInfo.InactiveSessionGeneration;
				var sessionAlreadyFixed = failedSessionGeneration < _session.CurrentSessionGeneration;

				if (sessionAlreadyInactive || sessionAlreadyFixed) {
					//This failure has already been reported
					//(it may or may not be corrected yet, but no action is needed to report it)
					return;
				}

				//You're the first thread to report this error so we must log out of this session so we can try again
				Logout(salesforceLogsRepository);
			}
		}

		public void Dispose() {
			Logout(null);
		}

		/// <summary>
		/// All login code is considered re-tryable. In case of failure any
		/// thread can try to grab a sesison again and re-try the login
		/// </summary>
		private void Login(ISalesforceLogsRepository salesforceLogsRepository) {
			try {
				_session.Binding = new SforceService();

				var configValues = new ConfigValues();

				_session.SalesforceUserName = configValues.GetAppSetting(AppSettings.SalesforceUser);

				var loginResult = _session.Binding.login(configValues.GetAppSetting(AppSettings.SalesforceUser),
																								configValues.GetAppSetting(AppSettings.SalesforcePassword));

				//You must establish the session after logging in
				_session.Binding.Url = loginResult.serverUrl;
				_session.SessionId = loginResult.sessionId;
				_session.Binding.SessionHeaderValue = new SessionHeader {
					sessionId = loginResult.sessionId,
				};

				_maxSessionGeneration++;
				_session.CurrentSessionGeneration = _maxSessionGeneration;
				try
				{
					salesforceLogsRepository.Log(_session.SessionId, _session.SalesforceUserName, string.Empty,
						SalesforceLogTypes.Login.ToString(), string.Empty, null);
				}
				catch (Exception e)
				{
                    EmailHelper.SendExceptionEmail(e);
				}

			} catch (Exception ex) {
				Logout(salesforceLogsRepository);
				throw new RetryAbleException(ex);
			}
		}

		private void Logout(ISalesforceLogsRepository salesforceLogsRepository) {

			if (salesforceLogsRepository != null) {
				salesforceLogsRepository.Log(_session.SessionId, _session.SalesforceUserName, string.Empty, SalesforceLogTypes.Logout.ToString(), string.Empty, null);
			}

			_session.SessionId = null;
			_session.CurrentSessionGeneration = SessionInfo.InactiveSessionGeneration;

			if (_session.Binding != null) {
				try {
					//_session.Binding.logout();
					//_session.Binding.Dispose();
				} catch (Exception ex) {
					Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				} finally {
					_session.Binding = null;
				}
			}
		}
	}

	public class SessionInfo {
        public SforceService Binding { get; set; }
        public string SessionId { get; set; }
		public string SalesforceUserName { get; set; }

        public long CurrentSessionGeneration { get; set; } = InactiveSessionGeneration;

        public const long InactiveSessionGeneration = -1;

		public bool IsActive() {
			return Binding != null && !String.IsNullOrEmpty(SessionId);
		}

        static SessionInfo() {
            // Salesforce requires TLS 1.1 or 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }
	}
}