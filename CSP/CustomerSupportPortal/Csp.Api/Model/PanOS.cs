using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model {
    
    public class PanOs {
        
       
        public string PanOsVersion { get; set; }
    }

    public class PanOsVersions
    {
        [JsonProperty("panOsVersions")]       
        public List<string> PanOsVersion { get; set; }
    }
}
