using System;

namespace Csp.Web.DataTransferObjects
{

    public class SoftwareUpdateListItemDto 
    {

        public int Id
        {
            set;
            get;
        }

        public string Key
        {
            set;
            get;
        }

        public string DisplayName
        {
            set;
            get;
        }


        public string VersionNumber
        {
            set;
            get;
        }

        public DateTime ReleaseDate
        {
            set;
            get;
        }

        public string ReleaseNotesType
        {
            set;
            get;
        }

        public string ReleaseNotesFileName
        {
            set;
            get;
        }

        public string ReleaseNotesUrl
        {
            set;
            get;
        }

        public string DownloadUrl
        {
            set;
            get;
        }


        public string FolderName
        {
            set;
            get;
        }

        public string FileName
        {
            set;
            get;
        }

       public string DisplayFileName { get; set; }

        public string FileSize
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
        

        public string PolicyFile
        {
            set;
            get;
        }


        public string PolicyFileUrl
        {
            set;
            get;
        }

        public string Platform
        {
            get;
            set;
        }

        public string SoftwareUpdateId { get; set; }

        public string Product {set;  get; }

        public int Order
        {
            get
            {
                switch (Key)
                {
                    case "BRIGHTCLOUD":
                        return 1;

                    default:
                        return 0;
                }
            }
        }
    }
}
