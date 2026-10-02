using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using RestSharp;
using System.IO;
using System.Reflection;
using System.Web.Configuration;

namespace Csp.Api.HelperClasses
{
    public static class Secrets
    {
        public static string CspConnectionString { get; set; }
        public static string EvalTrackerConnectionString { get; set; }

        public static string ContentPublishDatabase { get; set; }
        public static string SapHanaDb { get; set; }
        public static string CspDatabase { get; set; }
        public static string PanDatabase { get; set; }
        public static string PanLoginDatabase { get; set; }
        public static string MarketplaceDatabase { get; set; }
        public static string TempDatabase { get; set; }

        public static string EncryptionPasswordHash { get; }
        public static string EncryptionSaltKey { get; }
        public static string EncryptionViKey { get; }

        private static string ElmahConnectionString;
        private static Dictionary<string, string> _secretsDictionary;
        private static readonly string AppEnv = ConfigurationManager.AppSettings["AppEnv"];
        private static readonly string LocalDevSecretsFile = ConfigurationManager.AppSettings["LocalDevSecretsFile"];
        private static readonly string VaultUrl = ConfigurationManager.AppSettings["VaultUrl"];

        public static void Init()
        {
            try
            {
                // Get secrets from vault/local file
                var secretsJson = !string.IsNullOrEmpty(LocalDevSecretsFile)
                    ? System.IO.File.ReadAllText(LocalDevSecretsFile)
                    : GetSecretsFromVault();
                _secretsDictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(secretsJson);

                //check if correct Vault is loaded
                if (AppEnv != _secretsDictionary["AppEnvVerify"])
                    throw new Exception($"Got {_secretsDictionary["AppEnvVerify"]} vault secrets on {AppEnv} env.");

                // Set EF connection string

                CspConnectionString = _secretsDictionary["cspEntities"].Replace("&quot", "\"");

                EvalTrackerConnectionString = _secretsDictionary["EvalTrackerEntities"].Replace("&quot", "\"");

                CspDatabase = _secretsDictionary["CspDatabase"];
                BindDatabaseConnection("CspDatabase", CspDatabase);

                PanDatabase = _secretsDictionary["PANDatabase"];
                BindDatabaseConnection("PANDatabase", PanDatabase);

                MarketplaceDatabase = _secretsDictionary["MarketplaceDatabase"];
                BindDatabaseConnection("MarketplaceDatabase", MarketplaceDatabase);

                TempDatabase = _secretsDictionary["TempDatabase"];
                BindDatabaseConnection("TempDatabase", TempDatabase);


                // Set the Elmah connection string
                ElmahConnectionString = _secretsDictionary["Elmah"];

                // Set MachineKey from Vault for FormsAuthentication security
                SetMachineKey();

                // Set app settings
                foreach (var keyValuePair in _secretsDictionary)
                {
                    ConfigurationManager.AppSettings.Set(keyValuePair.Key, keyValuePair.Value);
                }
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, ex.ToString());
                throw new Exception("Vault Error: " + ex);
            }
        }

        private static string GetSecretsFromVault()
        {
            var client = new RestClient(VaultUrl);
            var request = new RestRequest();
            request.AddHeader("X-Vault-Request", "true");
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new Exception("Vault Secret Fetch Error:" + response.Content);

            var content = response.Content;
            dynamic json = JsonConvert.DeserializeObject(content);

            if (json == null || json.data == null || json.data.data == null)
                throw new Exception("Vault Secret Data Error:" + json);

            return json.data.data.ToString();
        }

        public static string GetElmahConnection()
        {
            return ElmahConnectionString;
        }

        public static void BindDatabaseConnection(string DatabaseName, string DatabaseConnection)
        {
            //Convert Readonly to Writable (In Connection String Provider is readonly so we need to change it as Writable)
            var settings = ConfigurationManager.ConnectionStrings[DatabaseName];
            var connectionConfiguration = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            if (connectionConfiguration == null)
                throw new Exception("connectionConfiguration is null. Check web.config for connection string entry.");
            connectionConfiguration.SetValue(settings, false);
            //Replace Empty Connecion string dynamically.
            settings.ConnectionString = DatabaseConnection;
        }

        /// <summary>
        /// Sets the MachineKey configuration from Vault secrets at runtime.
        /// This ensures each environment has unique encryption keys for FormsAuthentication,
        /// preventing cross-environment authentication ticket replay attacks.
        /// </summary>
        private static void SetMachineKey()
        {
            // Check if MachineKey values exist in Vault secrets
            if (!_secretsDictionary.ContainsKey("MachineKeyValidation") || 
                !_secretsDictionary.ContainsKey("MachineKeyDecryption"))
            {
                // Log warning but don't fail - allows gradual rollout
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\machinekey-warning-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                File.AppendAllText(logFile, DateTime.Now + " - MachineKey values not found in Vault. Using web.config defaults.\r\n");
                return;
            }

            var validationKey = _secretsDictionary["MachineKeyValidation"];
            var decryptionKey = _secretsDictionary["MachineKeyDecryption"];

            if (string.IsNullOrEmpty(validationKey) || string.IsNullOrEmpty(decryptionKey))
            {
                throw new Exception("MachineKey values in Vault are empty. Both MachineKeyValidation and MachineKeyDecryption must have values.");
            }

            try
            {
                // Get the system.web/machineKey section
                var config = WebConfigurationManager.OpenWebConfiguration("~");
                var machineKeySection = (MachineKeySection)config.GetSection("system.web/machineKey");

                if (machineKeySection == null)
                {
                    throw new Exception("MachineKey section not found in web.config");
                }

                // Use reflection to bypass the readonly restriction (same pattern as BindDatabaseConnection)
                var readOnlyField = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                if (readOnlyField == null)
                {
                    throw new Exception("Could not access _bReadOnly field for MachineKeySection");
                }

                readOnlyField.SetValue(machineKeySection, false);

                // Set the keys from Vault
                machineKeySection.ValidationKey = validationKey;
                machineKeySection.DecryptionKey = decryptionKey;
                machineKeySection.Validation = MachineKeyValidation.HMACSHA256;
                machineKeySection.Decryption = "AES";
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\machinekey-error-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
                File.AppendAllText(logFile, ex.ToString());
                throw new Exception("Failed to set MachineKey from Vault: " + ex.Message, ex);
            }
        }
    }
}
