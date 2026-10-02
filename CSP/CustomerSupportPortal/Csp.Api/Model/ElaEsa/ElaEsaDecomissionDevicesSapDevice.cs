using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDecomissionDevicesSapDevice
    {
        [JsonProperty("Support_Account_Id")]
        public long SupportAccountId { get; set; }

        [JsonProperty("User_Account_Id")]
        public long UserAccountId { get; set; }

        [JsonProperty("Serial_No")]
        public string SerialNumber { get; set; }       

        [JsonProperty("Decom_Category")]
        public int DecommissionCategory { get; set; }

        [JsonProperty("Sub_Category")]
        public int SubCategory { get; set; }

        [JsonProperty("Doc_Header_Text")]
        public string DocHeaderText { get; set; }

        [JsonProperty("Street")]
        public string Street { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Customer_Name")]
        public string CustomerName { get; set; }

        [JsonProperty("Postal_Code")]
        public string PostalCode { get; set; }

        [JsonProperty("RegionORState")]
        public string Region { get; set; }

        [JsonProperty("Device_Return")]
        public string RecycleDevice { get; set; }

        [JsonProperty("Phone_Number")]
        public string PhoneNumber { get; set; }

        public ElaEsaDecomissionDevicesSapDevice()
        {
        }

        public ElaEsaDecomissionDevicesSapDevice(ElaEsaDecomissionDevice device)
        {
            SupportAccountId = device.SupportAccountId;
            UserAccountId = device.UserAccountId;
            SerialNumber = device.SerialNumber;            
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
