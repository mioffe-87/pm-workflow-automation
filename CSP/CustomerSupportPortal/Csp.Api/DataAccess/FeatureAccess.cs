using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.ElasticMapReduce.Model;

namespace Csp.Api.DataAccess
{
    public class FeatureAccess : BaseDataAccess
    {
        public const string DecryptionPortMirror_Feature = "DPMEnabled";
        public const string DecryptionPortMirror_SKU = "PAN-PA-DECRYPT-MIRROR";
        public const string NetworkPacketBroker_Feature = "DPBEnabled";
        public const string SSLDecryptionBroker_SKU = "PAN-PA-DECRYPT-BROKER";
        public const string PanoramLicense_Feature = "PanoramaVMDeviceMgmtLicEnabled";

        /*Budapest Beta only.
        public const string VMRegistration_Feature = "VM-Registration";
        public const string VMGoogleCloudModel = "VM-GCE";
         */
        public const string PANOsVersion_Feature = "PANOSVersion";
        public const string PANOsBetaVersion = "9.0.0";


        //this is extracted from DpmRepository.IsModelDpmEnabled
        public bool IsFeatureEnabledForModel(string featureName, string model)
        {
            using (var command = Connection.GetStoredProcCommand("GetModelFeatureEnabled"))
            {
                Connection.AddInParameter(command, "@p_FeatureName", DbType.String, featureName);
                Connection.AddInParameter(command, "@p_Model", DbType.String, model);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public bool DeviceHasLicense(string serialNumber, string partNumber)
        {
            using (var command = Connection.GetStoredProcCommand("GetDeviceHasLicense"))
            {
                Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                Connection.AddInParameter(command, "@p_PartNumber", DbType.String, partNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return true;
                    }

                    return false;
                }
            }            
        }

        public bool IsBetaFeature(string featureName, string model)
        {
            using (var command = Connection.GetStoredProcCommand("GetBetaFeatureForModel"))
            {
                Connection.AddInParameter(command, "@p_FeatureName", DbType.String, featureName);
                Connection.AddInParameter(command, "@p_Model", DbType.String, model);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public bool CanUserAccessBetaFeature(string featureName, string userEmail, long? supportAccountId)
        {
            using (var command = Connection.GetStoredProcCommand("GetBetaUserAccessFeature"))
            {
                Connection.AddInParameter(command, "@p_FeatureName", DbType.String, featureName);
                Connection.AddInParameter(command, "@p_UserEmail", DbType.String, userEmail);
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public IEnumerable<string> GetBetaFeatureModels(string featureName)
        {
            var result = new List<string>();
            using (var command = Connection.GetStoredProcCommand("GetBetaFeatureModels"))
            {
                Connection.AddInParameter(command, "@p_FeatureName", DbType.String, featureName);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var model = dataReader["Model"].ToString();
                        result.Add(model);
                    }
                }
            }

            return result;
        }
    }
}
