using Amazon.OpsWorks.Model;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Attributes;
using Csp.Api.Services.Contracts;
using Google.Rpc;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;

namespace Csp.Api.Model {

	public class SoftwareUpdateListItem : AuditCspModel {

		[JsonProperty("id")]
		public int Id {
			set;
			get;
		}

	    public string Key
	    {
	        set;
	        get;
	    }

	    [JsonProperty("uniqueId")]
		public string UniqueId {
			set;
			get;
		}

        [JsonProperty("displayName")]
		public string DisplayName {
			set;
			get;
		}

        [JsonProperty("versionNumber")]
		public string VersionNumber {
			set;
			get;
		}

        [JsonProperty("releaseDate")]
		public DateTime ReleaseDate {
			set;
			get;
		}

        [JsonProperty("releaseNotesType")]
	    public string ReleaseNotesType
	    {
	        set;
	        get;
	    }

        [JsonProperty("releaseNotes")]
		public string ReleaseNotesFileName {
			set;
			get;
		}

        [JsonProperty("releaseNotesLink")]
		public string ReleaseNotesUrl {
			set;
			get;
		}

        [JsonProperty("platform")]
		public string Platform {
			set;
			get;
		}

        [JsonProperty("product")]
		public string Product {
			set;
			get;
		}

        [JsonProperty("downloadSite")]
		public string DownloadUrl {
			set;
			get;
		}

        
        public string FolderName
        {
            set;
            get;
        }

        [JsonProperty("fileName")]
		public string FileName {
			set;
			get;
		}

	    private string _fileSize;

        [JsonProperty("fileSize")]
		public string FileSize {
            get { return ApiCommon.GetSizeSuffix(_fileSize); }
            set { _fileSize = value; }
        }

        [JsonProperty("mD5Checksum")]
		public string Md5Key {
			set;
			get;
		}

        [JsonProperty("shA256Checksum")]
		public string Sha256Key {
			set;
			get;
		}

        [JsonProperty("policyFile")]
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
        
        public string SoftwareUpdateId { get; set; }

		public int Order {
            get {
                switch (Key) {
                    case "BRIGHTCLOUD":
                        return 1;

                    default:
                        return 0;
                }
            }
		}

	    public bool ShowReleaseNotes
	    {
            get { return !ReleaseNotesFileName.Equals("generic.txt"); }
            //get;
            //set;
	    }

        [JsonProperty("groupID")]
	    public string GroupId { get; set; }

	    public string MinimumRequiredVersion { get; set; }

        [JsonProperty("displayFileName")]
	    public string DisplayFileName { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("incremental")]
        public int Incremental { get; set; }

        [JsonProperty("modelsSupported")]
        public string ModelsSupported { get; set; }

        [JsonProperty("releaseType")]
        public string ReleaseType { get; set; }

        [JsonProperty("sbomFile")]
        public string SbomFile { get; set; }

        [JsonProperty("sbomDisplayName")]
        public string SbomDisplayName { get; set; }

        [JsonProperty("md5ChecksumSbom")]
        public string Md5ChecksumSbom { get; set; }

        [JsonProperty("sha256ChecksumSbom")]
        public string Sha256ChecksumSbom { get; set; }

        private string fileSizeSbom;
        [JsonProperty("fileSizeSbom")]
        public string FileSizeSbom
        {
            get { return ApiCommon.GetSizeSuffix(fileSizeSbom); }
            set { fileSizeSbom = value; }
        }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("currentVersionId")]
        public string CurrentVersionId { get; set; }
    
    }

    public class SoftwareUpdateFilters
    {
        [JsonProperty(PropertyName = "label")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string DisplayOrder { get; set; }

        public string Platform { get; set; }
        public string Product { get; set; }
        public string MinVersionRequired { get; set; }
        public string MaxVersionRequired { get; set; }
        public string ContentType { get; set; }
        [JsonProperty(PropertyName = "hasMultipleReleaseTypes")]
        public bool HasMultipleReleaseTypes { get; set; }
    }

    public class SoftwareUpdateRequest
    {
        public long SupportAccountId { get; set; }
        public long UserAccountId { get; set; }
        public string Platform { get; set; }
        public string Product { get; set; }
        public string GroupIds { get; set; }
        public string ContentType { get; set; }
        public string MinVersionRequired { get; set; }
        public string MaxVersionRequired { get; set; }
        public string Feature { get; set; }
        public string AdditionalFilter { get; set; }

        [JsonProperty("releaseType")]
        public string ReleaseType { get; set; }

        [JsonProperty("currentVersionId")]
        public string CurrentVersionId { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("hasMultipleReleaseTypes")]
        public bool HasMultipleReleaseTypes { get; set; }
    }

    public class SoftwareUpdateResponse
    {
        public bool success { get; set; }
        public string errorMessage { get; set; }
        //public List<SoftwareUpdateListItem> data { get; set; } //now changed
        public List<SoftwareUpdateModel> data { get; set; }
    }

    public class InputParams
    {
        public string contentType { get; set; }
        public List<queryFilters> queryFilters { get; set; }
    }

    public class queryFilters
    {
        public string filterName { get; set; }
        public string filterValue { get; set; }
        public string filterType { get; set; }
    }

    public enum ActionEnum
    {
        INSERT,
        GETContent,
        UPDATE,
        DELETE
    }

    public enum ContentTypeEnum
    {
        SOFTWARE,
        CONTENT,
        VIRUS,
        WILDFIRE,
        MDM,
        WFMETA,
        PANDB,
        GPCONTENT,
        PANDBSEED,
        PLUGININFO,
        TRAPS,
        BRIGHTCLOUD,
        PLUGIN,
        APPS,
        CONTENTS,
        CELLULAR, 
        ACE,
        REQUIRED_SW_UPGRADE,
        TRAFFICREDIRECTOR
    };

    public enum FilterNameEnum
    {
        ID,
        UNIQUEID,
        VERSIONNUMBER,
        FEATURE,
        GROUPID,
        ROWS,
        MINIMUMVERSIONREQUIRED,
        RELEASENOTES,
        FILENAME,
        PRODUCT,
        RELEASEDATE,
        PLATFORM,
        MAXSUPPORTEDVERSION,
        RELEASETYPE,
        CARRIER,
        PLATFORMMODEL
    };

    public enum FilterTypeEnum
    {
        EQUALS,
        NOTEQUALS,
        GREATER,
        LESSER,
        GREATEROREQUAL,
        LESSEROREQUAL,
        LIKE,
        LIKEALL,
        LIMIT,
        IN,
        ORDERBY
    };

    public enum ReleaseType
    {
        [Description("Preferred")]
        Preferred,
        [Description("EOL")]
        EOL,
        [Description("Certificate Expiration Remediated")]
        Remediated,
        [Description("Not tagged,Previously Preferred,")]
        Other,
        [Description("All")]
        All,
        [Description("Not tagged")]
        NotTagged,
        [Description("Previously Preferred")]
        PreviouslyPreferred,
    };

    public class DownloadUrlParams
    {
        public string folder { get; set; }
        public string file { get; set; }
        public string unit { get; set; }
        public string amount { get; set; }
    }

    public class DownloadUrlResponse
    {
        public bool success { get; set; }
        public string data { get; set; }
    }


    public enum Label

    {
        [Description("APPS")]
        Apps
    }
}
