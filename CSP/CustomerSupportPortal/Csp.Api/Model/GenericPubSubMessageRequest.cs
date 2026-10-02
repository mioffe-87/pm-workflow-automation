using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class GenericPubSubMessageRequest
    {
        public String topicName { get; set; }
        public String project { get; set; }

        public Object message {  get; set; }
    }
}
