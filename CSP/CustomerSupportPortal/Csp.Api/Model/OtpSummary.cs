using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class OtpSummary<T>
        where T: class
    {
        public long SupportAccountId { get; set; }

        public IEnumerable<T> Items { get; set; }

        public DataParameters Parameters { get; set; }
    }
}
