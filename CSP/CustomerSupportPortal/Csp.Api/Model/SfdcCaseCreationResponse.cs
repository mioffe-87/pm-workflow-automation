using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class SfdcCaseCreationResponse
    {
        public string CaseURL { get; set; }
        public string CaseNumber { get; set; }
        public string CaseId { get; set; }
    }
}
