namespace Csp.Api.Model
{
    public class SoftwareUpdatesChecksumModel
    {
        public string FileName
        {
            set;
            get;
        }

        public string FileSize
        {
            set;
            get;
        }

        public string VersionNumber
        {
            set;
            get;
        }

        public string Md5Key
        {
            set;
            get;
        }

        public string Sha256Key
        {
            set;
            get;
        }

        public bool HasMd5Key { get; set; }

        public bool HasSha256Key { get; set; }
    }
}
