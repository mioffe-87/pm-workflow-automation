using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class Preferences
    {
        public List<HideExpiredLicenses> HideExpiredLicenseses { get; set; }
    }


    public class HideExpiredLicenses
    {
        public bool Flag { get; set; }
        public long SupportAccountId { get; set; }
    }
}
