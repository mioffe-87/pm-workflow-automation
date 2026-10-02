using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    class OtpHistoryCsv
    {
        [CsvColumn(FieldIndex = 1, Name = "SerialNumber")]
        public string SerialNumber { get; set; }

        [CsvColumn(FieldIndex = 2, Name = "Device Type")]
        public string DeviceType { get; set; }

        [CsvColumn(FieldIndex = 3, Name = "OTP Type")]
        public string OTPType { get; set; }

        [CsvColumn(FieldIndex = 4, Name = "Status")]
        public string Status { get; set; }

        [CsvColumn(FieldIndex = 5, Name = "Expiration Date")]
        public string ExpirationDate { get; set; }

        [CsvColumn(FieldIndex = 6, Name = "Requestor")]
        public string Requestor { get; set; }
    }
}
