using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiNovaData
    {
        [JsonProperty("license")]
        public IEnumerable<WildfireApiNovaDataLicense> Licenses { get; set; }

        public IEnumerable<WildfireAssetItem> MapToAssets()
        {
            if (Licenses == null)
            {
                throw new ApplicationException("Nova GraphQL response Data.Licenses is NULL in MapToAssets.");
            }

            return Licenses.Select(i => i.MapToAssets()).Where(i => i != null).ToArray();
        }

        public IEnumerable<WildfireActivationLicense> MapToActivationLicenses()
        {
            if (Licenses == null)
            {
                throw new ApplicationException("Nova GraphQL response Data.Licenses is NULL in MapToActivationLicenses.");
            }

            return Licenses.Select(i => i.MapToActivationLicenses()).Where(i => i != null).ToArray();
        }
    }
}
