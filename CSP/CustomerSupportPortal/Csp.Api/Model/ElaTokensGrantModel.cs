using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class ElaTokensGrantModel
    {
				public long VmElaTokenGrantId { get; set; }
        public string AuthorizationCode { get; set; }
        public long SupportAccountId { get; set; }
        public String AccountName { get; set; }
        public DateTime GrantDate { get; set; }
        public string SourceUser { get; set; }        
    }
}
