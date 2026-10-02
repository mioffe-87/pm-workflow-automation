using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class OtpSummaryItem : OtpSummaryItemBase
    {
        public string SerialNumber { get; set; }

        public string DeviceType { get; set; }

        public string OTPType { get; set; }

        public string OTP { get; set; }

        public string Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}

