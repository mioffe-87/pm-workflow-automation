using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseResponse
    {
        [JsonProperty("data")]
        public ExtendedExpertiseResponseData Data { get; set; }

        [JsonProperty("errors")]
        public List<Xsoar.XsoarMarketplaceError> Errors { get; set; }

        public ExtendedExpertiseCoreData MapToCoreData(bool isSuperUser, string googleFormUrl, string creditMenuUrl)
        {
            return new ExtendedExpertiseCoreData
            {
                IsSuperUser = isSuperUser,
                GoogleFormUrl = isSuperUser ? googleFormUrl : string.Empty,
                CreditMenuUrl = creditMenuUrl,
                AvailableCreditBalance = Data != null && Data.Assets != null ? Data.Assets.Sum(i => i.QuantityNetBalance ?? 0D).ToString("N0") : "N/A"
            };
        }

        public IEnumerable<ExtendedExpertiseItem> MapToEngagements()
        {
            int count = 0;
            var eeList = Data != null && Data.UsageTransactions != null
                ? Data.UsageTransactions.Where(i => i.XrefCData != null && i.XrefCData.ExpertiseDetails != null).Select(
                    p => new ExtendedExpertiseItem
                    {
                        Id = ++count,
                        ConsultantName = p.XrefCData.ExpertiseDetails.ExpertiseConsultantName,
                        AccountTeam = p.XrefCData.ExpertiseDetails.AccountOwner,
                        ProjectType = p.XrefCData.ExpertiseDetails.ProjectType,
                        Status = p.XrefCData.ExpertiseDetails.State,
                        StartDate = p.XrefCData.ExpertiseDetails.EeStartDate,
                        EndDate = p.XrefCData.ExpertiseDetails.EeEndDate,
                        ProjectName = p.XrefCData.ExpertiseDetails.ProjectName,
                        MilestoneLastModifiedDate = p.XrefCData.ExpertiseDetails.MilestoneLastModifiedDate
                    }).ToArray()
             : new ExtendedExpertiseItem[0];

            return eeList.Where(i => i.ProjectName != null)
                .GroupBy(g => g.ProjectName)
                .Select(m => m.OrderByDescending(x => x.MilestoneLastModifiedDate)
                    .FirstOrDefault());
        }
    }
}
