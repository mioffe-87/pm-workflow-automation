using System.Security.Cryptography;
using System.Text;
using Csp.Api.Services.Contracts;
using System.IO;
using System;

namespace Csp.Api.Logic
{
    public class EncryptionProvider : IEncryptionProvider
    {
        public bool IsKeyValid(string value, string key, string privateKey)
        {
            var expectedKey = GetMd5Hash(value + privateKey);            
            return key == expectedKey;
        }

        public bool IsKeyValidAES(string value, string key, string privateKey)
        {            
            var expectedKey = GetAesHash(value + privateKey, privateKey);
            return key == expectedKey;
        }

        public string GetMd5Hash(string input)
        {

            var sBuilder = new StringBuilder();

            using (var md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.


                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (var i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public string GetSha256(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashstring = new SHA256Managed();
            var hash = hashstring.ComputeHash(bytes);
            var output = string.Empty;
            foreach (byte x in hash)
            {
                output += string.Format("{0:x2}", x);
            }
            return output;
        }


        public string GetAesHash(string clearText, string privateKey)
        {
            var clearBytes = Encoding.ASCII.GetBytes(clearText);

            using (var encrypt = Aes.Create())
            {
                var byteArray = Encoding.ASCII.GetBytes(privateKey);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt.CreateEncryptor(byteArray, byteArray), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }

            return clearText;
           
        }
    }
}
