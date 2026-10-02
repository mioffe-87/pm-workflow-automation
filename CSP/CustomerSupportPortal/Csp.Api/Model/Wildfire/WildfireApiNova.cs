using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiNova
    {
        [JsonProperty("data")]
        public WildfireApiNovaData Data { get; set; }

        public IEnumerable<WildfireAssetItem> MapToAssets()
        {
            List<WildfireActivationLicense> items = new List<WildfireActivationLicense>();

            if (Data == null)
            {
                throw new ApplicationException("Nova GraphQL response Data is NULL in MapToAssets.");
            }

            return Data.MapToAssets();
        }

        public IEnumerable<WildfireActivationLicense> MapToActivationLicenses()
        {
            List<WildfireActivationLicense> items = new List<WildfireActivationLicense>();

            if (Data == null)
            {
                throw new ApplicationException("Nova GraphQL response Data is NULL in MapToActivationLicenses.");
            }

            return Data.MapToActivationLicenses();
        }
    }
}
