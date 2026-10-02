using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Security.Attributes;

namespace Csp.Web.Controllers
{
		[Authorize]
    public class BootstrapContainerController : BaseController
    {
        private readonly IBootstrapContainerLogic _bootstrapContainerLogic;

        public BootstrapContainerController(IBootstrapContainerLogic bootstrapContainerLogic)
        {
            _bootstrapContainerLogic = bootstrapContainerLogic;
        }

        private const string UploadedFilesField = "UploadedFiles";
        private IEnumerable<HttpPostedFileBase> UploadedFiles
        {
            get
            {
                var obj = TempData[UploadedFilesField];
                if (obj == null)
                {
                    return new HttpPostedFileBase[0];
                }

                return (IEnumerable<HttpPostedFileBase>)TempData[UploadedFilesField];
            }
            set
            {
                TempData[UploadedFilesField] = value;
            }
        }

        private const string UploadedFilesAreValidatedField = "UploadedFilesAreValidated";
        private bool UploadedFilesAreValidated
        {
            get
            {
                var obj = Session[UploadedFilesAreValidatedField];
                return obj != null && (bool) obj;
            }
            set { Session[UploadedFilesAreValidatedField] = value; }
        }

        [CspAuthorize(Permission.EditDevice)]
        public ActionResult Index(long deviceId)
        {
            var model = new BootstrapContainerDto
            {
                DeviceId = deviceId,
            };

            model.SetGuid();

            UploadedFiles = null;
            UploadedFilesAreValidated = false;

            return PartialView("../Partial/_BootstrapContainer", model);
        }

        [CspAuthorize(Permission.EditDevice)]
        public ActionResult ValidateBoostrapContainerFiles(long deviceId, string guid)
        {
            UploadedFilesAreValidated = true;
            var model = _bootstrapContainerLogic.ValidateDownload(deviceId, User.UserAccount.Id, UploadedFiles.ToArray());

            var viewModel = new BootstrapContainerDto
            {
                DeviceId = deviceId,
            };
            viewModel.SetGuid(guid);

            if (model.IsSuccess)
            {
                viewModel.ShowDownloadLink = true;
                UploadedFiles = UploadedFiles;
            }
            else
            {
                UploadedFiles = null;

                viewModel.ErrorMessage = model.ErrorMessage;
            }

            return PartialView("../Partial/_BootstrapContainerDownload", viewModel);
        }

        [HttpGet, CspAuthorize(Permission.EditDevice)]
        public ActionResult Download(long deviceId)
        {
            var model = _bootstrapContainerLogic.PrepareDownload(deviceId, User.UserAccount.Id, UploadedFiles.ToArray());

            if (!model.IsSuccess)
            {
                throw new Exception(model.ErrorMessage);
            }

            const string fileName = "bootstrapcontainer_{0}_{1}.tar.gz";
            var date = DateTime.Now.ToString("yyyyMMdd");
            
            return File(model.DownloadStream.ToArray(), "application/gzip", string.Format(fileName, model.SerialNumber, date));
        }

        [HttpPost, CspAuthorize(Permission.EditDevice)]
        public ActionResult SaveFile(IEnumerable<HttpPostedFileBase> configFiles)
        {
            if (UploadedFilesAreValidated)
            {
                UploadedFiles = null;
                UploadedFilesAreValidated = false;
            }

            if (configFiles == null)
            {
                return Content("");
            }

            UploadedFiles = UploadedFiles.Union(configFiles);

            return Content("");
        }

        [HttpPost, CspAuthorize(Permission.EditDevice)]
        public ActionResult RemoveFile(string[] fileNames)
        {
            if (fileNames != null)
            {
                UploadedFiles = UploadedFiles.Where(f => !fileNames.Contains(f.FileName));
            }

            // Return an empty string to signify success
            return Content("");
        }

    }

    public class BootstrapContainerDto
    {
        public void SetGuid(string guid = null)
        {
            if (string.IsNullOrEmpty(guid) )
            {
                //start w/ letter
                //then guid
                //remove slashes
                guid = "x" + Guid.NewGuid().ToString().Replace("-", "");
            }
            ClearUploadFilesJsMethod = guid;
            FileUploadAreaDiv = "FileUploadArea" + guid;
        }

        public long DeviceId { get; set; }
        public string ErrorMessage { get; set; }
        public bool ShowDownloadLink { get; set; }
        public string ClearUploadFilesJsMethod { get; private set; }
        public string FileUploadAreaDiv { get; private set; }
    }
}
