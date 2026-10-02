using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Csp.Api.DataAccess;
using Csp.Api.Services.Contracts;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace Csp.Api.Logic
{
    public class BootstrapContainerLogic : IBootstrapContainerLogic
    {
        private readonly IBootstrapContainerRepository _bootstrapContainerRepository;

        public BootstrapContainerLogic(IBootstrapContainerRepository bootstrapContainerRepository)
        {
            _bootstrapContainerRepository = bootstrapContainerRepository;
        }

        private const long InvalidDeviceId = 0;

        public BootstrapContainerModel ValidateDownload(long deviceId, long userAccountId, HttpPostedFileBase[] uploadedFiles)
        {
            // 1) Verify user can see device in question
            var deviceDataAccess = new DeviceDataAccess();
            var dev = deviceDataAccess.Read(deviceId, userAccountId);

            if (dev == null || dev.Id == InvalidDeviceId)
            {
                return new BootstrapContainerModel
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid Device",
                };
            }

            // 2) Verify both attached files are valid
            const int maxUploadSizeInBytes = 1 * 1024 * 1024;//1MB

            var initConfigurationFile = uploadedFiles.FirstOrDefault(f => f.FileName.EndsWith(".txt"));
            if (initConfigurationFile == null || initConfigurationFile.ContentLength == 0)
            {
                return BootstrapError("Could not find an Init Configuration file");
            } else if (initConfigurationFile.ContentLength > maxUploadSizeInBytes)
            {
                return BootstrapError("Init Configuration file is too large");
            }


            var bootstrapConfigurationFile = uploadedFiles.FirstOrDefault(f => f.FileName.EndsWith(".xml"));
            if (bootstrapConfigurationFile == null || bootstrapConfigurationFile.ContentLength == 0)
            {
                //This isn't an error
            }
            else if (bootstrapConfigurationFile.ContentLength > maxUploadSizeInBytes)
            {
                return BootstrapError("Bootstrap Configuration file is too large");
            }

            
            

            return new BootstrapContainerModel
            {
                IsSuccess = true,
                SerialNumber = dev.SerialNumber,
            };
        }

        private BootstrapContainerModel BootstrapError(string message)
        {
            return new BootstrapContainerModel
            {
                IsSuccess = false,
                ErrorMessage = message,
            };
        }

        public BootstrapContainerModel PrepareDownload(long deviceId, long userAccountId, HttpPostedFileBase[] uploadedFiles)
        {
            var validate = ValidateDownload(deviceId, userAccountId, uploadedFiles);
            if (validate.IsSuccess == false)
            {
                return validate;
            }

            var initConfigurationFile = uploadedFiles.First(f => f.FileName.EndsWith(".txt"));
            var bootstrapConfigurationFile = uploadedFiles.FirstOrDefault(f => f.FileName.EndsWith(".xml"));

            // 3) Get relevant license key files
            var lics = _bootstrapContainerRepository.GetLicenseFiles(deviceId);

            // 4) Bundle license keys and attached files

            var stream = new MemoryStream();

            var gzoStream = new GZipOutputStream(stream);

            var tarOutputStream = new TarOutputStream(gzoStream);


            AddFolderToTarGz("software/", tarOutputStream);
            AddFolderToTarGz("content/", tarOutputStream);
            AddFolderToTarGz("license/", tarOutputStream);

            AddStreamToTarGz("config/" + initConfigurationFile.FileName, initConfigurationFile.InputStream, tarOutputStream);
            if (bootstrapConfigurationFile != null)
            {
                AddStreamToTarGz("config/" + bootstrapConfigurationFile.FileName, bootstrapConfigurationFile.InputStream,
                    tarOutputStream);
            }

            foreach (var lic in lics)
            {
                AddTextToTarGz("license/" + lic.FileName, 
                    lic.LicenseKey.Replace("\n", Environment.NewLine),
                    tarOutputStream);
            }

            tarOutputStream.IsStreamOwner = false;
            tarOutputStream.Close();

            gzoStream.IsStreamOwner = false;
            gzoStream.Close();

            stream.Position = 0;
            

            return new BootstrapContainerModel
            {
                IsSuccess = true,
                DownloadStream = stream,
                SerialNumber = validate.SerialNumber,
            };
        }

        private void AddFolderToTarGz(string fileName, TarOutputStream tarOutputStream)
        {
            var tarEntry = TarEntry.CreateTarEntry(fileName);
            tarEntry.Size = 0;
            tarOutputStream.PutNextEntry(tarEntry);
            tarOutputStream.CloseEntry();
        }

        private void AddStreamToTarGz(string fileName, Stream stream, TarOutputStream tarOutputStream)
        {
            var tarEntry = TarEntry.CreateTarEntry(fileName);
            tarEntry.Size = stream.Length;
            tarOutputStream.PutNextEntry(tarEntry);

            byte[] localBuffer = new byte[32 * 1024];
            while (true)
            {
                int numRead = stream.Read(localBuffer, 0, localBuffer.Length);
                if (numRead <= 0)
                {
                    break;
                }
                tarOutputStream.Write(localBuffer, 0, numRead);
            }

            tarOutputStream.CloseEntry();
        }

        private void AddTextToTarGz(string fileName, string fileContents, TarOutputStream tarOutputStream)
        {
            var bytes = ASCIIEncoding.ASCII.GetBytes(fileContents);
            using (var stream = new MemoryStream(bytes))
            {
                AddStreamToTarGz(fileName, stream, tarOutputStream);
            }
        }

        public bool ShouldShowDownloadButton(long deviceId, long restrictedByUserAccountId)
        {
            //only physical devices can use this feature
            return _bootstrapContainerRepository.ShouldShowDownloadButton(deviceId);
        }
    }
}
