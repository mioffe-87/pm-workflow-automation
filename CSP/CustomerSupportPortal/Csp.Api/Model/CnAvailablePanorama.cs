using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CnAvailablePanorama
    {
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string ModelName { get; set; }
    }
}