using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CspScheduledTasks.HelperClasses
{
    public class ApiCommon
    {

        public static string DecodeString(string value)
        {

            return value == null ? null : HttpUtility.HtmlDecode(value);
        }

        public static string EncodeString(string value)
        {

            if (value == null)
                return null;

            // Example encoding...
            // var inputChar = 'つ';
            // var charValue = (int)inputChar; // 12388
            // var htmlEncoded = "&#" + charValue + ";"; // &#12388;

            var chars = value.ToCharArray();
            var returnValue = "";
            foreach (var aChar in chars)
            {

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
    }
}
