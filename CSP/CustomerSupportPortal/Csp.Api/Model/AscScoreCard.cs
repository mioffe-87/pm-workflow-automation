using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model {
	public class AscScoreCard {

        public bool HasPartnerProfile { get; set; }
        public bool MultipleAsc { get; set; }

	    public PartnerProfile PartnerProfile { get; set; }
		public List<AscDeviceTheatrePartner> AscDeviceTheatrePartnerChart { get; set; }

		public List<InternalInformation> InternalDeviceInformationChart { get; set; }
		public List<InternalInformation> InternalCustomerInformationChart { get; set; }

		public ComplianceCurrentQtr FrontlineComplianceCurrentQuater { get; set; }
		public ComplianceCurrentQtr ResolutionRateComplianceCurrentQuater { get; set; }
		public ComplianceCurrentQtr CustomerSatComplianceCurrentQuater { get; set; }

		public List<MetricsData> FrontlineMetricsPartner { get; set; }
		public List<MetricsData> FrontlineMetrics { get; set; }
		public MetricsConsolidated FrontlineMetricsConsolidated { get; set; }

		public List<MetricsData> ResolutionMetricsPartner { get; set; }
		public List<MetricsData> ResolutionMetrics { get; set; }
		public MetricsConsolidated ResolutionMetricsConsolidated { get; set; }

		public List<PercentLineChartData> CsatTop3 { get; set; }
        public CsatLineChart CsatTop3LineChart { get; set; }

		public List<PercentLineChartData> CsatResponses { get; set; }
        public CsatLineChart CsatResponseLineChart { get; set; }
		public List<Comments> Comments { get; set; }
        public List<Comments> CommentsResultFromQuest { get; set; }

        public string LastRefreshDate { get; set; }

	    public AscScoreCard()
	    {
	        CsatTop3LineChart = new CsatLineChart();
	        CsatResponseLineChart = new CsatLineChart();
	    }
	}

    public class CsatLineChart
    {
        public List<MetricsChart> LineChart { get; set; }
        public string TheatreTextForChart { get; set; }
        public List<MultipleAscChart> SingleAscLineChart { get; set; }
        public List<MultipleAscChart> MultipleAscLineChart { get; set; }

        public CsatLineChart()
        {
            SingleAscLineChart = new List<MultipleAscChart>();
            MultipleAscLineChart = new List<MultipleAscChart>();
        }
    }

    public class DropDownModel
    {
        public string Value { get; set; }
        public string Id { get; set; }
    }

    public class AscPartner
    {
        public string AccountId { get; set; }
        public string PartnerName { get; set; }
        public string Theatre { get; set; }
    }

	public class PartnerProfile {
		public string PartnerName { get; set; }
		public string Type { get; set; }
		public string PartnerLevel { get; set; }
		public string Elite { get; set; }
		public string Theatre { get; set; }
		public string GlobalPartner { get; set; }
        public double BookingAmount { get; set; }
	}

	public class AscDeviceTheatrePartner {
		public string Theatre { get; set; }
		public string FiscalCode { get; set; }
		public string Category { get; set; }
		public int DeviceCount { get; set; }
		public int CustomerCount { get; set; }
	}



	public class InternalInformation {
		public string Category { get; set; }
		
		public int ApacCount { get; set; }
		public int AmericasCount { get; set; }
		public int EmeaCount { get; set; }
		public int GlobalCount { get; set; }
		public int JapanCount { get; set; }
	}

	public class MetricsData {
		public string Theatre { get; set; }
        public string PartnerName { get; set; }
		public string FiscalCode { get; set; }
		public string Category { get; set; }
		public int CaseCount { get; set; }
		public int TotalCount { get; set; }
		public Double PercentageTagged { get; set; }
		public string NonComplianceAlert { get; set; }
	}

	public class MetricsChart {
		public Double TargetCount { get; set; }
		public Double AmericasTotalCount { get; set; }
		public Double EmeaTotalCount { get; set; }
		public Double JapanTotalCount { get; set; }
		public Double ApacTotalCount { get; set; }
		public Double GLobalServicesTotalCount { get; set; }
		public Double PartnerTotalCount { get; set; }
		public string Category { get; set; }
	}

    public class MultipleAscChart {
        public Double TargetCount { get; set; }
        public Double WorldWideAverage { get; set; }
        public Double TotalCount { get; set; }
        public string Category { get; set; }
    }


	public class MetricsConsolidated {
		public List<string> Theatre { get; set; }
		public List<string> Category { get; set; }
        public List<string> PartnerList { get; set; }
		public List<MetricsData> MetricsList { get; set; }
		public List<MetricsData> MetricsPartner { get; set; }
		public List<MetricsChart> LineChart { get; set; }
        public string TheatreTextForChart { get; set; }
        public string PartnerTextForChart { get; set; }
        public List<MultipleAscChart> MultipleAscLineChart { get; set; }
	}

	public class LineChartData {
		public string Theatre { get; set; }
		public string FiscalCode { get; set; }
		public string Category { get; set; }
		public int Value { get; set; }
	}

	public class PercentLineChartData {
		public string Theatre { get; set; }
		public string FiscalCode { get; set; }
		public string Category { get; set; }
		public Double Value { get; set; }
	}

	public class ComplianceCurrentQtr {
		public string Alert { get; set; }
		public Double Actual { get; set; }
		public Double Target { get; set; }
		public Double TargetStart { get; set; }
		public Double TargetEnd { get; set; }
	}

	public class SoftwareEngineer {
		public string Contact_Name { get; set; }
		public string Contact_Email { get; set; }
		public int Total_Case_Count { get; set; }
		public int Frontline_Case_Count { get; set; }
		public Double Percentage_Tagged_Frontline { get; set; }
        public DateTime? Ace { get; set; }
        public DateTime? Pcnse { get; set; }
        public DateTime? Pcnsa { get; set; }
	    public bool NonCompliant { get; set; }
	    
	}

	public class Comments {
		public DateTime ResponseDate { get; set; }
		public string SurveyResponse { get; set; }
		public string CaseNumber { get; set; }
	}

	public class ConstantList
	{
		public int Id { get; set; }
		public string Value { get; set; }
	}

    public class AscDataModel
    {
        public long Id { get; set; }
        public long AscId { get; set; }
        public string SalesforceAccountId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public string CaseNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ProcessedMonth { get; set; }
    }

    public class LastRefreshDate
    {
        public DateTime RefreshDate { get; set; }
    }

    public class AscReminderEmail
    {
        public string AscSupportEmail { get; set; }
    }
}
