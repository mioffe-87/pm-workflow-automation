using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using Csp.Api.HelperClasses;
using opentoken;

namespace Csp.Web.HelperClasses {
	public static class UserHelper {
		public static IDictionary GetSsoToken() {
			IDictionary userInfo = null;

			using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(CorrectSsoValue()))) {
				var agent = new Agent(memoryStream);

				try {
					userInfo = agent.ReadToken(HttpContext.Current.Request);
				} catch {
					//These errors don't need to be logged
					//Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				}
			}

			return userInfo;
		}

		private static string CorrectSsoValue() {
			return ConfigurationManager.AppSettings["AgentConfigValue"];
		}

		public static string GetTokenByKey(string key, IDictionary token) {
			if (token != null && token.Contains(key)) {
				return (token[key] != null) ? token[key].ToString() : string.Empty;
			}

			return string.Empty;
		}

	    public static bool IsValidServiceAccount(string username, string password)
	    {
            return username == ApiCommon.Hash(ConfigurationManager.AppSettings["ServiceAccountUsername"]) &&
                   password == ApiCommon.Hash(ConfigurationManager.AppSettings["ServiceAccountPassword"]);
	    }
	}
}