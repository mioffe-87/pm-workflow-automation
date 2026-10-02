using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Csp.Api.Model
{
    public class JobStatusResponse : BaseJobResponse
    {
        [JsonProperty("jobs_info")]
        public Dictionary<long, List<JobStatusDetails>> JobsStatusInfo { get; set; }

    }

    public class JobStatusDetails
    {
        [JsonProperty("job_id")]
        public long JobId { get; set; }

        [JsonProperty("job_ts_filename")]
        public string TechSupportFileName { get; set; }

        [JsonProperty("job_serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("job_account_id")]
        public string AccountId { get; set; }

        [JsonProperty("job_contact_id")]
        public string ContactId { get; set; }

        [JsonProperty("job_product_code")]
        public string ProductCode { get; set; }

        [JsonProperty("job_distributor_id")]
        public string DistributorId { get; set; }

        [JsonProperty("job_end_customer_id")]
        public string EndCustomerId { get; set; }

        [JsonProperty("job_shipping_address")]
        public string ShippingAddress { get; set; }

        [JsonProperty("job_shipping_city")]
        public string ShippingCity { get; set; }

        [JsonProperty("job_shipping_state")]
        public string ShippingState { get; set; }

        [JsonProperty("job_shipping_country")]
        public string ShippingCountry { get; set; }

        [JsonProperty("job_shipping_postal_code")]
        public string ShippingPostalCode { get; set; }

        [JsonProperty("job_site_contact")]
        public string SiteContact { get; set; }

        [JsonProperty("job_site_email")]
        public string SiteEmail { get; set; }

        [JsonProperty("job_site_phone")]
        public string SitePhone { get; set; }

        [JsonProperty("job_status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus Status { get; set; }

        [JsonProperty("job_verdict")]
        [JsonConverter(typeof(StringEnumConverter))]
        public JobVerdict Verdict { get; set; }

        [JsonProperty("job_result_code")]
        public int? StatusCode { get; set; }

        [JsonProperty("job_result_msg")]
        public string StatusMessage { get; set; }

    }
}
