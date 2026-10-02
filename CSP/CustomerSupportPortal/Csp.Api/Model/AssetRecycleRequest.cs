using Csp.Api.Logic;
using Csp.Api.Model.ElaEsa;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class AssetRecycleRequest
    {
        [JsonProperty("support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty("user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty("serial_no")]
        public string SerialNumber { get; set; }

        [JsonProperty("material_no")]
        public string Material { get; set; }

        [JsonProperty("decom_category")]
        public int DecommissionCategory { get; set; }

        [JsonProperty("sub_category")]
        public int SubCategory { get; set; }

        [JsonProperty("doc_header_text")]
        public string DocHeaderText { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("customer_name")]
        public string CustomerName { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("region_or_state")]
        public string Region { get; set; }

        [JsonProperty("device_return")]
        public string RecycleDevice { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        public AssetRecycleRequest()
        {
        }

        public AssetRecycleRequest(ElaEsaDecomissionDevice device)
        {
            SupportAccountId = device.SupportAccountId;
            UserAccountId = device.UserAccountId;
            SerialNumber = device.SerialNumber;
            Material = device.Material;
            DecommissionCategory = device.DecommissionCategory;
            SubCategory = device.SubCategory;
            DocHeaderText = device.DocHeaderText;
            Street = device.Street;
            City = device.City;
            Country = device.Country;
            Email = device.Email;
            CustomerName = device.Name;
            PostalCode = device.ZipCode;
            Region = device.Region;
            RecycleDevice = device.RecycleDevice ? "Y" : "N";
            PhoneNumber = device.PhoneNumber;
        }
    }
}
