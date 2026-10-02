using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Csp.Api.Model
{
    public class ConfigModel
    {
		  [JsonProperty("value")]
	      public int LevelId { get; set; }

	      [JsonProperty("label")]
	      public string LevelName { get; set; }

		  public int ParentLevelId { get; set; }
    }
}
