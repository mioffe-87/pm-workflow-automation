using System;
using System.Security.Cryptography.X509Certificates;

namespace Csp.Api.HelperClasses
{
    public class CertificateManager
    {
        public static X509Certificate2 GetCertificate(string thumbprint, string privateKey = "") {
			if (string.IsNullOrWhiteSpace(thumbprint)) return null;

			thumbprint = System.Text.RegularExpressions.Regex.Replace(thumbprint, @"[^\da-fA-F]", string.Empty).ToUpper();
			var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);

			store.Open(OpenFlags.ReadOnly);

			var collection = store.Certificates.Find(X509FindType.FindByThumbprint,
				thumbprint, true);

			X509Certificate2 certificate = null;

			if (collection.Count == 1) {
				certificate = collection[0];
			} else {
				throw new Exception("Unable to locate the correct client certificate.");
			}

			store.Close();
			return certificate;
		}

        public static X509Certificate2 GetCertificateByCnName(string cnName, string privateKey = "")
        {
            if (string.IsNullOrWhiteSpace(cnName)) return null;

            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var collection = store.Certificates.Find(X509FindType.FindBySubjectName,
                cnName, true);
            
            X509Certificate2 certificate = null;
            if (collection.Count == 1)
            {
                certificate = collection[0];
            }
            else
            {
                throw new Exception("Unable to locate the correct client certificate:" + cnName );
            }

            store.Close();
            return certificate;
        }
    }
}

