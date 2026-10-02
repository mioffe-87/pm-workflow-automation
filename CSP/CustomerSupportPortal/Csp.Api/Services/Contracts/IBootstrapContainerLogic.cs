using System.Web;

namespace Csp.Api.Services.Contracts
{
    public interface IBootstrapContainerLogic
    {
        BootstrapContainerModel PrepareDownload(long deviceId, long userAccountId, HttpPostedFileBase[] uploadedFiles);
        BootstrapContainerModel ValidateDownload(long deviceId, long userAccountId, HttpPostedFileBase[] uploadedFiles);
        bool ShouldShowDownloadButton(long deviceId, long restrictedByUserAccountId);
    }
}
