using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class WildfireApiRequest
    {
        public long supportAccountId { get; set; }

        public string serialNumber { get; set; }

        public string productType { get; set; }

        public string sourceType { get; set; }

        [JsonConverter(typeof(WFDateTimeConverter))]
        public DateTime startDate { get; set; }

        [JsonConverter(typeof(WFDateTimeConverter))]
        public DateTime endDate { get; set; }
    }

    public class WFDateTimeConverter : IsoDateTimeConverter
    {
        public WFDateTimeConverter()
        {
            base.DateTimeFormat = "MM/dd/yyyy hh:mm:ss";
        }
    }
}
