using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseItem
    {
        private const string Z_DATETIME_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

        private string startDate = null;
        private string endDate = null;

        public long Id { get; set; }

        public string ConsultantName { get; set; }

        public string AccountTeam { get; set; }

        public string ProjectType { get; set; }

        public string ProjectName { get; set; }

        public DateTime MilestoneLastModifiedDate { get; set; }

        public string StartDate
        {
            get
            {
                return string.IsNullOrWhiteSpace(startDate)
                    ? string.Empty
                    : DateTime.ParseExact(startDate, Z_DATETIME_FORMAT, CultureInfo.InvariantCulture).ToLocalTime().ToShortDateString();
            }
            set
            {
                startDate = value;
            }
        }

        public string EndDate
        {
            get
            {
                return string.IsNullOrWhiteSpace(endDate)
                    ? string.Empty
                    : DateTime.ParseExact(endDate, Z_DATETIME_FORMAT, CultureInfo.InvariantCulture).ToLocalTime().ToShortDateString();
            }
            set
            {
                endDate = value;
            }
        }

        public string Status { get; set; }

        public static string CsvHeader
        {
            get
            {
                return string.Format("ConsultantName,AccountTeam,ProjectType,StartDate,EndDate,Status");
            }
        }

        public string CsvRow
        {
            get
            {
                return string.Format("{0},{1},{2},{3},{4},{5}", ConsultantName, AccountTeam, ProjectType, StartDate, EndDate, Status);
            }
        }
    }
}
