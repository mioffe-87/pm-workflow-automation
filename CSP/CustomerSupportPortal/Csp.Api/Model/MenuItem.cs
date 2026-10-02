using System.Collections.Generic;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class MenuItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("iconClass")]
        public string IconClass { get; set; }

        [JsonIgnore]
        public bool IsExternalLink { get; set; }

        [JsonProperty("target")]
        public string Target
        {
            get { return IsExternalLink ? "_blank" : "_top"; }
        }

        [JsonProperty("submenu")]
        public IEnumerable<MenuItem> SubMenu { get; set; }
    }

    public class QuickActionLinkItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("orderBy")]
        public string OrderBy { get; set; }

        [JsonProperty("iconClass")]
        public string IconClass { get; set; }

        [JsonProperty("isExternalLink")]
        public bool IsExternalLink { get; set; }

        [JsonProperty("linkText")]
        public string LinkText { get; set; }

        [JsonProperty("isCaseCreation")]
        public bool IsCaseCreation { get; set; }


    }

}
