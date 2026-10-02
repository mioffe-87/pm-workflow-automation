using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;

namespace Csp.Api.Model
{
    public class CsspProvisionedDevices
    {
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string DeviceName { get; set; }
        public string ProvisionedDate { get; set; }
        public string EndUserCustomerAccountId { get; set; }
        public string EndUserCompany { get; set; }
        public string EndUserContactEmail { get; set; }
        public string EndUserAddress { get; set; }
        public string EndUserCountry { get; set; }
        public string EndUserState { get; set; }
        public string EndUserCity { get; set; }
        public string EndUserPostalCode { get; set; }
        public string EndUserPhoneNumber { get; set; }
        public string EndUserCompanyWebsite { get; set; }
        public string EndUserIndustry { get; set; }
        public string DnbNumber { get; set; }
        public bool HasEndUserInfo { get; set; }
        public string MasterAuthCode { get; set; }

        public string EndUserFullAddress
        {
            get
            {
                if (string.IsNullOrEmpty(EndUserAddress))
                {
                    return string.Empty;
                }
                else
                {
                    return "<div>" + EndUserAddress + ", " + EndUserCity + ", " + EndUserState + " " + EndUserPostalCode + ", " + EndUserCountry + "</div>";   
                }
            }
        }
    }

    public class CsspProvisionedDevicesCsv
    {
        [CsvColumn(FieldIndex = 2, Name = "Provisioned Serial Number")]
        public string SerialNumber { get; set; }

        [CsvColumn(FieldIndex = 3, Name = "Model")]
        public string Model { get; set; }

        public string DeviceName { get; set; }

        [CsvColumn(FieldIndex = 4, Name = "Provisioned Date")]
        public string ProvisionedDate { get; set; }

        [CsvColumn(FieldIndex = 5, Name = "CSSP Customer Reference Id")]
        public string EndUserCustomerAccountId { get; set; }

        [CsvColumn(FieldIndex = 6, Name = "Customer DNB Number")]
        public string DnbNumber { get; set; }

        [CsvColumn(FieldIndex = 7, Name = "End User Company Name")]
        public string EndUserCompany { get; set; }

        [CsvColumn(FieldIndex = 8, Name = "End User Contact Email")]
        public string EndUserContactEmail { get; set; }

       [CsvColumn(FieldIndex = 9, Name = "Country")]
        public string EndUserCountry { get; set; }

        [CsvColumn(FieldIndex = 10, Name = "State")]
        public string EndUserState { get; set; }

        [CsvColumn(FieldIndex = 11, Name = "City")]
        public string EndUserCity { get; set; }

        [CsvColumn(FieldIndex = 12, Name = "Postal Code")]
        public string EndUserPostalCode { get; set; }

        [CsvColumn(FieldIndex = 1, Name = "Authorization Code")]
        public string MasterAuthCode { get; set; }

        [CsvColumn(FieldIndex = 13, Name = "PhoneNumber")]
        public string EndUserPhoneNumber { get; set; }

        [CsvColumn(FieldIndex = 14, Name = "Website")]
        public string EndUserCompanyWebsite { get; set; }

        [CsvColumn(FieldIndex = 15, Name = "Industry")]
        public string EndUserIndustry { get; set; }

    }
}
