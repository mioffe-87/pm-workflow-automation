using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class GenericPubSubMessageResponse
    {
        public string messageId { get; set; }
        public string message { get; set; }
        public string topicName { get; set; }
        public bool success { get; set; }
    }
}
