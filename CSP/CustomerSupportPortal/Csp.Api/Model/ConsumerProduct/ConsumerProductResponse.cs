using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ConsumerProduct
{
    public class ConsumerProductResponse<T>
        where T: class, new()
    {
        [JsonProperty("items")]
        public IEnumerable<T> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
