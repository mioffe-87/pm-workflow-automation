using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Api.Model
{
    public class DeviceIonDetailsLightInfo
    {
        [JsonProperty("DeviceId", Required = Required.Always)]
        public long DeviceId { get; set; }

        [JsonProperty("SerialNumber", Required = Required.AllowNull)]
        public string SerialNumber { get; set; }

        [JsonProperty("Model", Required = Required.AllowNull)]
        public string Model { get; set; }

        [JsonProperty("DeviceName", Required = Required.AllowNull)]
        public string DeviceName { get; set; }

        [JsonProperty("TagId", Required = Required.AllowNull)]
        public long? TagId { get; set; }

        [JsonProperty("PanOsVersion", Required = Required.AllowNull)]
        public string PanOsVersion { get; set; }

        [JsonProperty("Address1", Required = Required.AllowNull)]
        public string Address1 { get; set; }

        [JsonProperty("Address2", Required = Required.AllowNull)]
        public string Address2 { get; set; }

        [JsonProperty("City", Required = Required.AllowNull)]
        public string City { get; set; }

        [JsonProperty("CountryId", Required = Required.AllowNull)]
        public long? CountryId { get; set; }

        [JsonProperty("Country", Required = Required.AllowNull)]
        public string Country { get; set; }

        [JsonProperty("Region", Required = Required.AllowNull)]
        public string Region { get; set; }

        [JsonProperty("PostalCode", Required = Required.AllowNull)]
        public string PostalCode { get; set; }

        [JsonProperty("Comments", Required = Required.AllowNull)]
        public string Comments { get; set; }

        [JsonProperty("Tags", Required = Required.AllowNull)]
        public IEnumerable<dynamic> Tags { get; set; }

        [JsonProperty("ShowLicenseTransfer", Required = Required.AllowNull)]
        public bool ShowLicenseTransfer { get; set; }

        [JsonProperty("TenantId", Required = Required.AllowNull)]
        public string TenantId { get; set; }
    }
}