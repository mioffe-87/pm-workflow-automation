using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Logic;
using Csp.Api.Model;

namespace Csp.Web.HelperClasses {

	public class Common {

		public static string GetClientIp() {
			var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (string.IsNullOrEmpty(ip)) {
				ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
			}
			return ip;
		}

		public static string GetSizeSuffix(double value) {
			string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
			var i = 0;
			var dValue = (decimal)value;

			while (Math.Round(dValue / 1024) >= 1) {
				dValue /= 1024;
				i++;
			}

			return string.Format("{0:n1} {1}", dValue, suffixes[i]);
		}

		public static string GetCurrentDeviceTitle() {
			string title = CurrentState.GetDeviceName();
			bool nameExists = (title != null && !title.Trim().Equals(""));
		    if (nameExists)
		        title += " (";
		    else
		        title = string.Empty;
            var sn = CurrentState.GetDeviceSerialNumber();
		    if (string.IsNullOrEmpty(sn))
		        sn = string.Empty;
		    title += sn;
			if (nameExists)
				title += ")";
			return title;
		}

	    public static string Sha256(string password)
	    {
	        string pwd = string.Format("{0}{1}", password, System.DateTime.Now.ToString());
	        SHA256Managed crypt = new SHA256Managed();
	        string hash = String.Empty;
	        byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(pwd), 0, Encoding.ASCII.GetByteCount(pwd));
	        foreach (byte theByte in crypto)
	        {
	            hash += theByte.ToString("x2");
	        }
	        return hash;
	    }
	}
}