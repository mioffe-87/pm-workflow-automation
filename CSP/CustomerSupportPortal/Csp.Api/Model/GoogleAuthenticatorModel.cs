using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class GoogleAuthenticatorModel
    {
        public string UserEmail { get; set; }
        public string SecretKey { get; set; }
        public string BarcodeUrl { get; set; }
        public string Code { get; set; }
    }
}
