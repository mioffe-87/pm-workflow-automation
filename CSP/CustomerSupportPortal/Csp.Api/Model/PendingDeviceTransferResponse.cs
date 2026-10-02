using Csp.Api.Logic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class PendingDeviceTransferResponse
    {
        [JsonProperty("data")]
        public IEnumerable<PendingDeviceTransferModel> PendingDeviceTransfers { get; set; }
    }

    public class PendingDeviceTransferModel
    {
        [JsonProperty("device_transfer_id")]
        public long DeviceTransferId { get; set; }

        [JsonProperty("device_id")]
        public long DeviceId { get; set; }

        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("device_name")]
        public string DeviceName { get; set; }

        [JsonProperty("part_number")]
        public string PartNumber { get; set; }

        [JsonProperty("device_transfer_begin_date")]
        public DateTime DeviceTransferBeginDate { get; set; }

        [JsonProperty("from_user_accountId")]
        public long FromUserAccountId { get; set; }

        [JsonProperty("from_user_display_name")]
        public string FromUserDisplayName { get; set; }

        [JsonProperty("device_transfer_state")]
        public string DeviceTransferState { get; set; }

        [JsonProperty("source_support_account_id")]
        public string SourceSupportAccountId { get; set; }
    }
}
