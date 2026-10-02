using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;

namespace Csp.Api.Model
{
    public class CsspProvisionedDevicesMonthly
    {
        [CsvColumn(FieldIndex = 1, Name = "Palo Alto Networks Product Code")]
        public string PartNumber { get; set; }

        [CsvColumn(FieldIndex = 2, Name = "Provisioned Serial Number")]
        public string SerialNumber { get; set; }

        [CsvColumn(FieldIndex = 3, Name = "Start Date")]
        public string StartDate { get; set; }

        [CsvColumn(FieldIndex = 4, Name = "End Date")]
        public string EndDate { get; set; }

        [CsvColumn(FieldIndex = 5, Name = "Provisioned Date")]
        public string ProvisionedDate { get; set; }

        [CsvColumn(FieldIndex = 6, Name = "Deactivation Date")]
        public string DeactivationDate { get; set; }

        [CsvColumn(FieldIndex = 7, Name = "Term End Date")]
        public string TermEndDate { get; set; }

        [CsvColumn(FieldIndex = 8, Name = "Authorization Code")]
        public string MasterAuthCode { get; set; }

        [CsvColumn(FieldIndex = 9, Name = "Model")]
        public string Model { get; set; }

        [CsvColumn(FieldIndex = 10, Name = "CSSP Customer Reference Id")]
        public string EndUserCustomerAccountId { get; set; }

        [CsvColumn(FieldIndex = 11, Name = "Customer DNB Number")]
        public string DnbNumber { get; set; }

        [CsvColumn(FieldIndex = 12, Name = "End User Company Name")]
        public string EndUserCompany { get; set; }

        [CsvColumn(FieldIndex = 13, Name = "End User Contact EMail")]
        public string EndUserContactEmail { get; set; }

        [CsvColumn(FieldIndex = 14, Name = "Country")]
        public string EndUserCountry { get; set; }

        [CsvColumn(FieldIndex = 15, Name = "State")]
        public string EndUserState { get; set; }

        [CsvColumn(FieldIndex = 16, Name = "City")]
        public string EndUserCity { get; set; }

        [CsvColumn(FieldIndex = 17, Name = "Postal Code")]
        public string EndUserPostalCode { get; set; }

        [CsvColumn(FieldIndex = 18, Name = "Phone Number")]
        public string EndUserPhoneNumber { get; set; }

        [CsvColumn(FieldIndex = 19, Name = "Website")]
        public string EndUserCompanyWebsite { get; set; }

        [CsvColumn(FieldIndex = 20, Name = "Industry")]
        public string EndUserIndustry { get; set; }

        [CsvColumn(FieldIndex = 21, Name = "Device Name")]
        public string DeviceName { get; set; }
    }
}
