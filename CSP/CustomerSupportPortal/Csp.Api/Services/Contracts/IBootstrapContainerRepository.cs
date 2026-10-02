using System.Collections.Generic;

namespace Csp.Api.Services.Contracts
{
    public interface IBootstrapContainerRepository
    {
        bool ShouldShowDownloadButton(long deviceId);
        IEnumerable<BootstrapContainerLics> GetLicenseFiles(long deviceId);
    }

    public class BootstrapContainerLics
    {
        public string LicenseKey { get; set; }

        public string SerialNumber { private get; set; }
        public string LicenseFeaturesName { private get; set; }

        public string FileName
        {
            get { return string.Format("{0}-{1}.key", SerialNumber, LicenseFeaturesName); }
        }
    }
}