using Csp.Api.HelperClasses;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {
	public class ConfigValues : IConfigValues {

		public string GetAppSetting(string name) {
			return System.Configuration.ConfigurationManager.AppSettings[name];
		}

		public string GetAppSetting(AppSettings appSetting) {
			return System.Configuration.ConfigurationManager.AppSettings[appSetting.ToString()];
		}
	    public static void InitConfig()
	    {
	        Secrets.Init();
	    }

	    public static string GetElmahConnectionString()
	    {
	        return Secrets.GetElmahConnection();
	    }
	}
}