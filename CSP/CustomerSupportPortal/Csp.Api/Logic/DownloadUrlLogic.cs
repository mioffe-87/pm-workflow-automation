using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {
	public class DownloadUrlLogic : IDownloadUrlLogic {

		private readonly IConfigValues _configValues;
		private readonly bool _cdnEnabled;
		private readonly string _cdnHostname;
		private readonly string _cdnUrlFormat;
		private readonly string _cdnSalt;
		private readonly string _cdnExtract;
		private readonly long _cdnWindow;
		private readonly string _swReleaseNotesUrl;
		private readonly string _swDownloadUrl;
		private readonly string _siReleaseNotesUrl;
		private readonly string _siDownloadUrl;
		private readonly string _svReleaseNotesUrl;
		private readonly string _svDownloadUrl;
		private readonly string _wfReleaseNotesUrl;
		private readonly string _wfDownloadUrl;
		private readonly string _wfmetaReleaseNotesUrl;
		private readonly string _wfmetaDownloadUrl;
	    private readonly string _trapsDownloadUrl;
        private readonly string _gpDownloadUrl;
        private readonly string _eusaFileURL;
        private readonly string _eulaFileURL;
        private readonly bool _isQA;
	    private readonly string _QAFolder = string.Empty;

		public DownloadUrlLogic(IConfigValues configValues) {
			_configValues = configValues;

			_cdnEnabled = Convert.ToBoolean(_configValues.GetAppSetting(AppSettings.CdnEnabled));
			_cdnHostname = _configValues.GetAppSetting(AppSettings.CdnHostname);
			_cdnUrlFormat = _configValues.GetAppSetting(AppSettings.CdnUrlFormat);
			_cdnSalt = _configValues.GetAppSetting(AppSettings.CdnSalt);
			_cdnExtract = _configValues.GetAppSetting(AppSettings.CdnExtract);
			_cdnWindow = Convert.ToInt64(_configValues.GetAppSetting(AppSettings.CdnWindow));
			_swReleaseNotesUrl = _configValues.GetAppSetting(AppSettings.swReleaseNotesURL);
			_swDownloadUrl = _configValues.GetAppSetting(AppSettings.swDownloadURL);
			_siReleaseNotesUrl = _configValues.GetAppSetting(AppSettings.siReleaseNotesURL);
			_siDownloadUrl = _configValues.GetAppSetting(AppSettings.siDownloadURL);
			_svReleaseNotesUrl = _configValues.GetAppSetting(AppSettings.svReleaseNotesURL);
			_svDownloadUrl = _configValues.GetAppSetting(AppSettings.svDownloadURL);
			_wfReleaseNotesUrl = _configValues.GetAppSetting(AppSettings.wfReleaseNotesURL);
			_wfDownloadUrl = _configValues.GetAppSetting(AppSettings.wfDownloadURL);
			_wfmetaReleaseNotesUrl = _configValues.GetAppSetting(AppSettings.wfmetaReleaseNotesURL);
			_wfmetaDownloadUrl = _configValues.GetAppSetting(AppSettings.wfmetaDownloadURL);
            _trapsDownloadUrl = _configValues.GetAppSetting(AppSettings.trapsDownloadURL);
            _gpDownloadUrl = _configValues.GetAppSetting(AppSettings.gpDownloadURL);
            _eusaFileURL = _configValues.GetAppSetting(AppSettings.eusaFileURL);
            _eulaFileURL = _configValues.GetAppSetting(AppSettings.eulaFileURL);
            
            // QA automation
			_isQA = _configValues.GetAppSetting(AppSettings.AppName) == "CSP TEST" || _configValues.GetAppSetting(AppSettings.AppName) == "CSP DEV";
		    if (_isQA)
		    {
                var folder = _configValues.GetAppSetting(AppSettings.QAFolder);
		        if (!string.IsNullOrEmpty(folder))
		            _QAFolder = folder;
		    }
		}

		public void PopulateSoftwareUpdatesDownloadUrl(SoftwareUpdateModel model) {
			if (_cdnEnabled) {

                if (model.Product.ToLower() == "plugin" || model.Product.ToLower() == "vm-series plugin" || model.Product.ToLower() == "pluginv2")
                {
                    // Comments By LK On 12/14/2017
                    // Generate the token on the filename click event
                    //model.DownloadUrl = GetCdnUrl("plugins", model.FileName, -1);
                    //model.ReleaseNotesUrl = GetCdnUrl("plugins", model.ReleaseNotes, -1);
                    //model.PolicyFileUrl = GetCdnUrl("plugins", model.PolicyFile, -1);
                    model.FolderName = "plugins";
                }
                else
                {
                    // Comments By LK On 12/14/2017
                    // Generate the token on the filename click event
                    //model.DownloadUrl = GetCdnUrl("software", model.FileName, -1);
                    //model.ReleaseNotesUrl = GetCdnUrl("software", model.ReleaseNotes, -1);
                    //model.PolicyFileUrl = GetCdnUrl("software", model.PolicyFile, -1);
                    model.FolderName = "software";
                }			        
			    
			    //Add ReleaseNotes Link if ReleaseNotesType is not File
			   model.ReleaseNotesUrl = model.ReleaseNotesType != null ? (model.ReleaseNotesType.Equals(ReleaseNotesType.HyperLink.ToString()) ? model.ReleaseNotesUrl : string.Empty) : string.Empty;

			} else {

				model.DownloadUrl = string.Format(_swDownloadUrl, model.FileName);
				model.ReleaseNotesUrl = model.ReleaseNotesType.Equals(ReleaseNotesType.File.ToString()) ? 
				    String.Format(_swReleaseNotesUrl, "sw", model.VersionNumber, model.Product, model.Platform) : model.ReleaseNotesUrl;
                model.PolicyFileUrl = string.Format(_swDownloadUrl, model.PolicyFile);
			}
		}

        public void PopulateCellulerUpdatesDownloadUrl(SoftwareUpdateModel model)
        {
            if (_cdnEnabled)
            {
                model.FolderName = "cellular";
                model.ReleaseNotesUrl = model.ReleaseNotesType != null ? (model.ReleaseNotesType.Equals(ReleaseNotesType.HyperLink.ToString()) ? model.ReleaseNotesUrl : string.Empty) : string.Empty;
            }
            else
            {
                model.DownloadUrl = string.Format(_swDownloadUrl, model.FileName);
                model.ReleaseNotesUrl = model.ReleaseNotesType.Equals(ReleaseNotesType.File.ToString())
                    ? String.Format(_swReleaseNotesUrl, "sw", model.VersionNumber, model.Product, model.Platform)
                    : model.ReleaseNotesUrl;
                model.PolicyFileUrl = string.Format(_swDownloadUrl, model.PolicyFile);
            }
        }

        public void PopulateEulaDownloadUrl(SimpleEula model)
        {
            model.EulaDownloadUrl = _eulaFileURL;
            model.EusaDownloadUrl = _eusaFileURL;
        }

        public string GetDownloadUrl(string folder, string filename)
        {
            return GetCdnUrl(!string.IsNullOrEmpty(folder) && folder.ToLower() == "plugin"? "plugins":folder, filename, 30);
        }

	    public void PopulateDynamicUpdatesDownloadUrl(DynamicUpdateModel model) {

            string folder = string.Empty; 
            string foldercode = string.Empty;
            string localDownloadUrl = string.Empty;
            string localReleaseNotesUrl = string.Empty;

            //switch (model.Product.ToLower()) {
            //    case "apps":
            //    case "contents":
            //        folder = "content";
            //        foldercode = "si";
            //        localDownloadUrl = _siDownloadUrl;
            //        localReleaseNotesUrl = _siReleaseNotesUrl;
            //        break;

            //    case "virus":
            //        folder = "virus";
            //        foldercode = "sv";
            //        localDownloadUrl = _svDownloadUrl;
            //        localReleaseNotesUrl = _svReleaseNotesUrl;
            //        break;

            //    case "wildfire":
            //        folder = "wildfire";
            //        foldercode = "wf";
            //        localDownloadUrl = _wfDownloadUrl;
            //        localReleaseNotesUrl = _wfReleaseNotesUrl;
            //        break;

            //    case "wf-500 content":
            //        folder = "wfmeta";
            //        foldercode = "wfmeta";
            //        localDownloadUrl = _wfmetaDownloadUrl;
            //        localReleaseNotesUrl = _wfmetaReleaseNotesUrl;
            //        break;

            //    case "brightcloud":
            //        folder = "brightcloud";
            //        foldercode = "si";
            //        localDownloadUrl = _siDownloadUrl;
            //        localReleaseNotesUrl = _siReleaseNotesUrl;
            //        break;

            //    case "pandb":
            //        folder = "pandb";
            //        foldercode = "si";
            //        localDownloadUrl = _siDownloadUrl;
            //        localReleaseNotesUrl = _siReleaseNotesUrl;
            //        break;

            //    case "traps":
            //        folder = "traps";
            //        foldercode = "ep";
            //        localDownloadUrl = _siDownloadUrl;
            //        localReleaseNotesUrl = _siReleaseNotesUrl;
            //        break;

            //    case "gpcontents":
            //        folder = "gpcontent";
            //        foldercode = "si";
            //        localDownloadUrl = _gpDownloadUrl;
            //        localReleaseNotesUrl = "";
            //        break;

            //    default:
            //        folder = "software";
            //        foldercode = "sw";
            //        localDownloadUrl = _swDownloadUrl;
            //        localReleaseNotesUrl = _swReleaseNotesUrl;
            //        break;
			//}

			if (_cdnEnabled) {
			    if (_isQA)
			    {
			        var folderName = string.Format("{0}/{1}", _QAFolder, folder);
                    // Comments By LK On 12/14/2017
                    // Generate the token on the filename click event
			        //model.DownloadUrl = GetCdnUrl(folderName, model.FileName, -1);
			        //model.ReleaseNotesUrl = (folder.Equals("brightcloud") || folder == "wfmeta" || folder == "pandb" || model.ReleaseNotes == "generic.txt")
			        //    ? string.Empty : GetCdnUrl(folderName, model.ReleaseNotes, -1);
                    model.FolderName = folderName;
			    }
			    else
			    {
                    // Comments By LK On 12/14/2017
                    // Generate the token on the filename click event
                    //model.DownloadUrl = GetCdnUrl(folder, model.FileName, -1);
                    //model.ReleaseNotesUrl = (folder.Equals("brightcloud") || folder == "wfmeta" || folder == "pandb" || model.ReleaseNotes == "generic.txt") 
                    //    ? string.Empty : GetCdnUrl(folder, model.ReleaseNotes, -1);
                    model.FolderName = folder;
                }
			}
            else
            {
				var releaseNotesUrl = folder.Equals("brightcloud") ? string.Empty : localReleaseNotesUrl;

				model.DownloadUrl = string.Format(localDownloadUrl, model.FileName);
				model.ReleaseNotesUrl = String.Format(releaseNotesUrl, foldercode, model.VersionNumber, model.Product, model.Platform);
                model.FolderName = folder;
			}
		}

		private string GetCdnUrl(string folder, string filename, long window) {
			var url = string.Format(_cdnUrlFormat, folder, filename);
            // Hack to remove dup entry of "QA/QA/" in URL until we fix the use of suffix/prefix in folders paths. 
            // This is temporary solution to avoid breaking other code, which uses relative UNC parts.
            // TODO: rewrite the hack, which suppressing "QA/QA/" dup in URL.
            if (!string.IsNullOrWhiteSpace(_QAFolder))
            {
                url = url.Replace(string.Format("{0}/{0}/", _QAFolder), string.Format("{0}/", _QAFolder));
            }

			var token = new UrlToken(url, window <= 0 ? _cdnWindow : window, _cdnSalt, _cdnExtract, 0, "");
			return _cdnHostname + token.AuthUrl;
		}
	}

	public class UrlToken {
		private static string myVersion = "1.1.2";

		private string myToken;

		private string myUrl;
		private string mySalt;
		private string myExtract;
		private string myParam;

		private long myWindow;
		private long myTime;
		private long myExpires;

		public string String {
			get {
				return myToken;
			}
		}

		public string AuthUrl {
			get {
				var sb = new StringBuilder();

				sb.Append(myUrl);

				sb.Append(myUrl.IndexOf("?") < 0 ? "?" : "&");

				sb.Append(myParam);
				sb.Append("=");
				sb.Append(myExpires);
				sb.Append("_");
				sb.Append(myToken);

				return sb.ToString();
			}
		}

		public static string Version {
			get {
				return myVersion;
			}
		}

		public UrlToken(string inUrl, long inWindow, string inSalt,
										string inExtract, long inTime, string inParam) {

			if (string.IsNullOrEmpty(inUrl)) {
				throw (new TokenException("URL is empty or null"));
			}

			if (inWindow < 0) {
				throw (new TokenException("Window is negative"));
			}

			if (string.IsNullOrEmpty(inSalt)) {
				throw (new TokenException("Salt is empty or null"));
			}

			myUrl = inUrl;
			myWindow = inWindow;
			mySalt = inSalt;

			if (string.IsNullOrEmpty(inExtract)) {
				myExtract = null;
			} else {
				myExtract = inExtract;
			}

			if (inTime <= 0) {
				myTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			} else {
				myTime = inTime;
			}

			if (string.IsNullOrEmpty(inParam)) {
				myParam = "__gda__";
			} else {
				myParam = inParam;
			}

			if (myParam.Length < 5 || myParam.Length > 12) {
				throw (new TokenException("Parameter must be between 5 and 12 characters in length"));
			}

			myExpires = myTime + myWindow;

			var expBytes = new byte[4];
			expBytes[0] = Convert.ToByte(myExpires & 0xff);
			expBytes[1] = Convert.ToByte((myExpires >> 8) & 0xff);
			expBytes[2] = Convert.ToByte((myExpires >> 16) & 0xff);
			expBytes[3] = Convert.ToByte((myExpires >> 24) & 0xff);

			var sb = new StringBuilder();

			var encoding = new ASCIIEncoding();

			sb.Append(myUrl);
			sb.Append(myExtract);
			sb.Append(mySalt);

			var dataBytes = encoding.GetBytes(sb.ToString());

			var buffer1 = new byte[expBytes.Length + sb.Length];
			Array.Copy(expBytes, 0, buffer1, 0, expBytes.Length);
			Array.Copy(dataBytes, 0, buffer1, expBytes.Length, dataBytes.Length);

			var md5 = new MD5CryptoServiceProvider();

			var digest1 = md5.ComputeHash(buffer1);

			var saltBytes = encoding.GetBytes(mySalt);

			var buffer2 = new byte[saltBytes.Length + digest1.Length];
			Array.Copy(saltBytes, 0, buffer2, 0, saltBytes.Length);
			Array.Copy(digest1, 0, buffer2, saltBytes.Length, digest1.Length);

			var digest2 = md5.ComputeHash(buffer2);

			var tokenCore = new StringBuilder();

			for (int i = 0; i < digest2.Length; i++) {
				var s = Convert.ToString(digest2[i], 16);
				if (s.Length == 1) {
					tokenCore.Append("0" + s);
				} else {
					tokenCore.Append(s);
				}
			}

			myToken = tokenCore.ToString();
		}
	}

	public class TokenException : Exception {

		public TokenException()
			: base() {
		}

		public TokenException(string msg)
			: base(msg) {
		}
	}
}