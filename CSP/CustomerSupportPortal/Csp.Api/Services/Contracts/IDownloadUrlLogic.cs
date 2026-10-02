using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface IDownloadUrlLogic {
		void PopulateSoftwareUpdatesDownloadUrl(SoftwareUpdateModel model);
		void PopulateDynamicUpdatesDownloadUrl(DynamicUpdateModel model);
	    void PopulateEulaDownloadUrl(SimpleEula model);
	    string GetDownloadUrl(string folder, string filename);
        void PopulateCellulerUpdatesDownloadUrl(SoftwareUpdateModel model);
    }
}