using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Csp.Web.HelperClasses
{
    public static class SecurityHelper
    {
        private static readonly string Secret = ConfigurationManager.AppSettings["SecurityHelperEncryptionSecretKey"];

        public static string Encrypt(string clearText)
        {
            var encryptionKey = Secret;
            var clearBytes = Encoding.Unicode.GetBytes(clearText);

            using (var encrypt = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encrypt.Key = pdb.GetBytes(32);
                encrypt.IV = pdb.GetBytes(16);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }

            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            string encryptionKey = Secret;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (var encrypt = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encrypt.Key = pdb.GetBytes(32);
                encrypt.IV = pdb.GetBytes(16);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string ConvertStringToHex(string input)
        {
            try
            {
                Byte[] stringBytes = Encoding.Unicode.GetBytes(input);
                StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
                foreach (byte b in stringBytes)
                {
                    sbBytes.AppendFormat("{0:X2}", b);
                }
                return sbBytes.ToString();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return string.Empty;
            }
           
        }

        public static string ConvertHexToString(string hexInput)
        {
            try
            {
                int numberChars = hexInput.Length;
                byte[] bytes = new byte[numberChars / 2];
                for (int i = 0; i < numberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hexInput.Substring(i, 2), 16);
                }
                return Encoding.Unicode.GetString(bytes);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return string.Empty;
            }
        }

        public static string ComputeSha256Hash(string text)
        {
            // Create a SHA256   
            using (var sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

                // Convert byte array to a string   
                var builder = new StringBuilder();

                foreach (var t in bytes)
                {
                    builder.Append(t.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static string Base64Encode(string input)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(plainTextBytes);
        }
        
        public static string Base64Decode(string encodedInput)
        {
            var base64EncodedBytes = Convert.FromBase64String(encodedInput);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string CustomReverseEncode(string input) {
            string base64EncodedString = Base64Encode(input);
            char[] charArray = base64EncodedString.ToCharArray();
            Array.Reverse(charArray);
            string reversedString = new string(charArray);
            //string withoutEqualChars = reversedString.Substring(2, reversedString.Length-2);
            return reversedString;
        }
    }
}