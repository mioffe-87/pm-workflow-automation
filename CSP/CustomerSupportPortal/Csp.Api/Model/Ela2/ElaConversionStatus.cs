using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Ela2
{
    public class ElaConversionStatus : ElaConversionStatusBase
    {
        [JsonIgnore]
        public EElaConversionStatus Status { get { return (EElaConversionStatus)Enum.Parse(typeof(EElaConversionStatus), StatusValue); } }

        [JsonProperty("status")]
        public string StatusValue { get; set; }
    }
}
