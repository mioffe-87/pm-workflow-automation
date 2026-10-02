using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class AlertInfo
    {
        //id: item.Title,
        //name: item.Summary,
        //url: item.URL

        [JsonProperty("id")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("name")]
        public string Summary { get; set; }
    }

    public class AlertGroups
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconClass")]
        public string IconClass { get; set; }

        [JsonProperty("items")]
        public List<AlertInfo> Items { get; set; }
    }

    public class Notifications
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("groups")]
        public List<AlertGroups> Groups { get; set; }
    }

    public class CaseActivity
    {

        public string Title { get; set; }

        public string URL { get; set; }

       public string Summary { get; set; }
    }
}
