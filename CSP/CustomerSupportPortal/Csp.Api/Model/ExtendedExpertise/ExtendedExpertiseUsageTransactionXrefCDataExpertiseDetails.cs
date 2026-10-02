using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseUsageTransactionXrefCDataExpertiseDetails
    {
        [JsonProperty("accountOwner")]
        public string AccountOwner { get; set; }

        [JsonProperty("eeStartDate")]
        public string EeStartDate { get; set; }

        [JsonProperty("eeEndDate")]
        public string EeEndDate { get; set; }

        [JsonProperty("expertiseConsultantName")]
        public string ExpertiseConsultantName { get; set; }

        [JsonProperty("expertiseSpeciality")]
        public string ExpertiseSpeciality { get; set; }

        [JsonProperty("projectType")]
        public string ProjectType { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("milestoneLastModifiedDate")]
        public DateTime MilestoneLastModifiedDate { get; set; }
    }
}
