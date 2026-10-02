using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Csp.Api.HelperClasses {

	public class ApiCommon {

		public static string DecodeString(string value) {

			return value == null ? null : HttpUtility.HtmlDecode(value);
		}

		public static string EncodeString(string value) {

			if (value == null)
				return null;

			// Example encoding...
			// var inputChar = 'つ';
			// var charValue = (int)inputChar; // 12388
			// var htmlEncoded = "&#" + charValue + ";"; // &#12388;

			var chars = value.ToCharArray();
			var returnValue = "";
			foreach (var aChar in chars) {

				int charValue = (int)aChar;
				returnValue += (charValue > 127) ? "&#" + charValue + ";" : aChar + "";
			}
			return returnValue;
		}

        public static string Hash(string value)
        {
            // Convert string into bytes using a text encoder
            var encoder = Encoding.ASCII.GetEncoder();

            // Create a buffer large enough to hold the string
            var data = new byte[value.Length];
            encoder.GetBytes(value.ToCharArray(), 0, value.Length, data, 0, true);

            MD5 md5 = new MD5CryptoServiceProvider();
            var result = md5.ComputeHash(data);

            return BitConverter.ToString(result).ToLower();
        }

	    public static string GetSizeSuffix(string strValue)
	    {
	        double value = 0;

	        var dValue = value;

	        string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
	        var i = 0;

	        if (!string.IsNullOrEmpty(strValue))
	        {
	            double.TryParse(strValue, out value);
	            // var dValue = (decimal)value;
	            dValue = value;

	            while (Math.Round(dValue / 1024) >= 1)
	            {
	                dValue /= 1024;
	                i++;
	            }
	        }

	        if (i >= suffixes.Length)
	            i = suffixes.Length -1 ;

	        return string.Format("{0:n1} {1}", dValue, suffixes[i]);
	    }

        public static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }

        public static long SecondsSinceUnixEpoch
        {
            get
            {
                return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
        }

        public static long ToUnixEpochDate(DateTime date) {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }

	    public static byte[] Compress(byte[] dataStream)
	    {
	        using (var compressedStream = new MemoryStream())
	        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
	        {
                zipStream.Write(dataStream, 0, dataStream.Length);
	            zipStream.Close();
	            return compressedStream.ToArray();
	        }
	    }

        public static byte[] Decompress(byte[] dataStream)
	    {
            using (var compressedStream = new MemoryStream(dataStream))
	        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
	        using (var resultStream = new MemoryStream())
	        {
	            var buffer = new byte[4096];
	            int read;
	            while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
	            {
	                resultStream.Write(buffer, 0, read);
	            }
	            return resultStream.ToArray();
	        }
	    }

        public static bool IsNumericFromTryParse(string str)
        {
            double result = 0;
            return (double.TryParse(str, System.Globalization.NumberStyles.Float,
                    System.Globalization.NumberFormatInfo.CurrentInfo, out result));
        }

        public static double ToNumericFromTryParse(string str)
        {
            double result = 0;
            double.TryParse(str, System.Globalization.NumberStyles.Float,
                    System.Globalization.NumberFormatInfo.CurrentInfo, out result);
            return result;
        }

        public static string EncryptStringToBytes(string plainText, string key, string iv)
        {
            string encrypted;
            using (var cipher = new RijndaelManaged())
            {
                cipher.Padding = PaddingMode.PKCS7;
                cipher.FeedbackSize = 128;
                var bytesKey = Encoding.UTF8.GetBytes(key);
                cipher.Key = bytesKey;
                cipher.IV = Encoding.UTF8.GetBytes(iv);
                var encryptor = cipher.CreateEncryptor(cipher.Key, cipher.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            return encrypted;
        }

    }
}

