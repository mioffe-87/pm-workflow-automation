using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDecomedDevice
    {
        public const string CVS_HEADER = "Decommission Date,Serial Number,SKU,Model,Asset Name,ELA Auth Code,ELA List Price,ESA Auth Code,ESA List Price,Status,Requested By";

        [JsonProperty("id")]
        public long DeviceId { get; set; }

        [JsonProperty("decomDate")]
        public DateTime? DecomDate { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("assetName")]
        public string AssetName { get; set; }

        [JsonProperty("elaAuthCodes")]
        public IEnumerable<string> ElaAuthCodes { get; set; }

        [JsonProperty("elaListPrices")]
        public IEnumerable<double> ElaListPrices { get; set; }

        [JsonProperty("esaAuthCodes")]
        public IEnumerable<string> EsaAuthCodes { get; set; }

        [JsonProperty("esaListPrices")]
        public IEnumerable<double> EsaListPrices { get; set; }

        [JsonProperty("status")]
        public EElaEsaActionStatus Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("requestedBy")]
        public string User { get; set; }

        [JsonProperty("isRecycle")]
        public bool isRecycle { get; set; }

        [JsonIgnore]
        public string CsvData
        {
            get
            {
                return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}\r\n",
                    DecomDate, SerialNumber, Sku, Model, AssetName,
                    ElaAuthCodes?.Any() ?? false ? string.Join("|", ElaAuthCodes) : string.Empty,
                    ElaListPrices?.Any() ?? false ? string.Join("|", ElaListPrices) : string.Empty,
                    EsaAuthCodes?.Any() ?? false ? string.Join("|", EsaAuthCodes) : string.Empty,
                    EsaListPrices?.Any() ?? false ? string.Join("|", EsaListPrices) : string.Empty,
                    Status, User);
            }
        }
    }
}