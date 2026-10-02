using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{
    public class ContactInformation
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Company { get; set; }
    }

    public class ContactLocation
    {
        public ContactInformation ContactDetails { get; set; }
        public LocationDetail LocationDetails { get; set; }
    }
}