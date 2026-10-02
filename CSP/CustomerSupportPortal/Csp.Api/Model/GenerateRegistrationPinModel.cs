using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class GenerateRegistrationPinRequest
    {
        public long SupportAccountId { get; set; }

        public long UserAccountId { get; set; }

        public string UserEmail { get; set; }

        public string Description { get; set; }

        public int PinExpirationInDays { get; set; }
    }


    public class GenerateRegistrationPinResponse
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string RegistrationPinId { get; set; }

        public string RegistrationPin { get; set; }

        public string ExpirationDate { get; set; }
    }
}
