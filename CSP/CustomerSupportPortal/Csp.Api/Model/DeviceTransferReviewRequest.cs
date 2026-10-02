using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class DeviceTransferReviewRequest
    {
        [JsonProperty(PropertyName = "device_transfer_id")]
        public long DeviceTransferId { get; set; }

        [JsonProperty(PropertyName = "device_id")]
        public long DeviceId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "destination_support_account_id")]
        public long DestinationSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "transfer_state")]
        public string TransferState { get; set; }

        [JsonProperty(PropertyName = "eula_id")]
        public int EulaId { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
}
