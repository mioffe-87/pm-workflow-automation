
using System.Web.Services.Protocols;

namespace Csp.Web.HelperClasses
{
    public class AuthHeader : SoapHeader
    {
        public string Username;
        public string Password;
    }
}
