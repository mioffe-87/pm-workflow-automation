using Csp.Api.Logic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class DeviceTransferRequest
    {
        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "device_name")]
        public string DeviceName { get; set; }

        [JsonProperty(PropertyName = "part_number")]
        public string PartNumber { get; set; }

        [JsonProperty(PropertyName = "device_id")]
        public long DeviceId { get; set; }

        [JsonProperty(PropertyName = "source_support_account_id")]
        public long SourceUserAccountId { get; set; }

        [JsonProperty(PropertyName = "destination_email")]
        public string DestinationEmail { get; set; }

        [JsonProperty(PropertyName = "notify_user")]
        public bool NotifyUser { get; set; }

        [JsonProperty(PropertyName = "transfer_by")]
        public string TransferBy { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "has_asc_transfer_access")]
        public bool HasAscTransferAccess { get; set; }

        [JsonProperty(PropertyName = "has_lgs_associated")]
        public bool HasLgsAssociated { get; set; }
    }

    public class DeviceTransferPayload
    {
        [JsonProperty(PropertyName = "device_id")]
        public long DeviceId { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "destination_user_email")]
        public string DestinationUserEmail { get; set; }

        [JsonProperty(PropertyName = "destination_support_account_id")]
        public long? DestinationSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "eula_id")]
        public string EulaId { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }


        [JsonProperty(PropertyName = "notify_user")]
        public bool NotifyUser { get; set; }
    }

    public class DeviceTransferBulkRequest
    {
        [JsonProperty(PropertyName = "source_support_account_id")]
        public long SourceUserAccountId { get; set; }

        [JsonProperty(PropertyName = "destination_support_account_id")]
        public long DestinationSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "transferred_by_user_account_id")]
        public long TransferredByUserAccountId { get; set; }

        [JsonProperty(PropertyName = "device_ids")]
        public long[] DeviceId { get; set; }
    }
}
