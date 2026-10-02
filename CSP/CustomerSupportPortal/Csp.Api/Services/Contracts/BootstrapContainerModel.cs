using System.IO;

namespace Csp.Api.Services.Contracts
{
    public class BootstrapContainerModel
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public MemoryStream DownloadStream { get; set; }
        public string SerialNumber { get; set; }
    }
}