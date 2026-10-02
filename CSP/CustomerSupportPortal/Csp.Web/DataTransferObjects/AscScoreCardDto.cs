using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects {
	public class AscScoreCardDto : DefaultDto {

		public bool IsLastFiveQtr {get;set;}
		public bool IsInternal { get; set; }
        public string SFDCAccountId { get; set; }
        public string Theatre { get; set; }
        public string IsincludeCurrentPartial { get; set; }

        public bool HasPartnerProfile { get; set; }
        public bool MultipleAsc { get; set; }

		public PartnerProfileDto PartnerProfile { get; set; }

		public List<InternalInformationDto> InternalDeviceInformationChart { get; set; }
		public List<InternalInformationDto> InternalCustomerInformationChart { get; set; }

		public List<AscDeviceTheatrePartnerDto> AscDeviceTheatrePartnerChart { get; set; }

		public ComplianceCurrentQtrDto FrontlineComplianceCurrentQuater { get; set; }
		public ComplianceCurrentQtrDto ResolutionRateComplianceCurrentQuater { get; set; }
		public ComplianceCurrentQtrDto CustomerSatComplianceCurrentQuater { get; set; }

		public MetricsConsolidatedDto FrontlineMetricsConsolidated { get; set; }
		public MetricsConsolidatedDto ResolutionMetricsConsolidated { get; set; }

        public CsatLineChartDto CsatTop3LineChart { get; set; }
        public CsatLineChartDto CsatResponseLineChart { get; set; }
		public List<CommentsDto> Comments { get; set; }
        public List<CommentsDto> CommentsResultFromQuest { get; set; }

        public string LastRefreshDate { get; set; }

	    public AscScoreCardDto()
	    {
	        CsatTop3LineChart = new CsatLineChartDto();
	        CsatResponseLineChart = new CsatLineChartDto();
	    }
	}

    public class CsatLineChartDto
    {
        public List<MetricsChartDto> LineChart { get; set; }
        public string TheatreTextForChart { get; set; }
        public List<MultipleAscChartDto> SingleAscLineChart { get; set; }
        public List<MultipleAscChartDto> MultipleAscLineChart { get; set; }

        public CsatLineChartDto()
        {
            SingleAscLineChart = new List<MultipleAscChartDto>();
            MultipleAscLineChart = new List<MultipleAscChartDto>();
        }
    }

    public class DropDownModelDto
    {
        public string Value { get; set; }
        public string Id { get; set; }
    }

	public class AscDeviceTheatrePartnerDto {
		public string Theatre { get; set; }
		public string Category { get; set; }
		public int DeviceCount { get; set; }
		public int CustomerCount { get; set; }
	}


	public class InternalInformationDto {
		public string Category { get; set; }

		public int ApacCount { get; set; }
		public int AmericasCount { get; set; }
		public int EmeaCount { get; set; }
		public int GlobalCount { get; set; }
		public int JapanCount { get; set; }
	}

	public class PartnerProfileDto {
		public string PartnerName { get; set; }
		public string Type { get; set; }
		public string PartnerLevel { get; set; }
		public string Elite { get; set; }
		public string Theatre { get; set; }
		public string GlobalPartner { get; set; }
        public double BookingAmount { get; set; }
	}

	public class MetricsDataDto {
		public string Theatre { get; set; }
        public string PartnerName { get; set; }
		public string Category { get; set; }
		public int CaseCount { get; set; }
		public int TotalCount { get; set; }
		public Double PercentageTagged { get; set; }
		public string NonComplianceAlert { get; set; }
	}

	public class MetricsChartDto {
		public Double TargetCount { get; set; }
		public Double AmericasTotalCount { get; set; }
		public Double EmeaTotalCount { get; set; }
		public Double JapanTotalCount { get; set; }
		public Double ApacTotalCount { get; set; }
		public Double GLobalServicesTotalCount { get; set; }
		public Double PartnerTotalCount { get; set; }
		public string Category { get; set; }
	}

    public class MultipleAscChartDto{
        public Double TargetCount { get; set; }
        public Double WorldWideAverage { get; set; }
        public Double TotalCount { get; set; }
        public string Category { get; set; }
    }

	public class MetricsConsolidatedDto {
		public List<string> Theatre { get; set; }
		public List<string> Category { get; set; }
        public List<string> PartnerList { get; set; }
		public List<MetricsDataDto> MetricsList { get; set; }
		public List<MetricsDataDto> MetricsPartner { get; set; }
		public List<MetricsChartDto> LineChart { get; set; }
        public string TheatreTextForChart { get; set; }
        public string PartnerTextForChart { get; set; }
        public List<MultipleAscChartDto> MultipleAscLineChart { get; set; }
	}

	public class ComplianceCurrentQtrDto {
		public string Alert { get; set; }
		public Double Actual { get; set; }
		public Double Target { get; set; }
		public Double TargetStart { get; set; }
		public Double TargetEnd { get; set; }
	}

	public class SoftwareEngineerDto {
		public string Contact_Name { get; set; }
		public string Contact_Email { get; set; }
		public int Total_Case_Count { get; set; }
		public int Frontline_Case_Count { get; set; }
		public Double Percentage_Tagged_Frontline { get; set; }
        public DateTime? Ace { get; set; }
        public DateTime? TrafficRedirector { get; set; }
        public DateTime? Pcnse { get; set; }
        public DateTime? Pcnsa { get; set; }
        public bool NonCompliant { get; set; }

	    public string PcnseCompliance
	    {
	        get { return Pcnse != null ? ValidateCertificationCompliance(Pcnse.Value) : string.Empty; }
	    }

	    public string PcnsaCompliance
	    {
            get { return Pcnsa != null ? ValidateCertificationCompliance(Pcnsa.Value) : string.Empty; }
	    }

        public string AceCompliance
        {
            get { return Ace != null ? ValidateCertificationCompliance(Ace.Value) : string.Empty; }
        }

        public string TrafficRedirectorCompliance
        {
            get { return Ace != null ? ValidateCertificationCompliance(TrafficRedirector.Value) : string.Empty; }
        }

        private string ValidateCertificationCompliance(DateTime certDate)
	    {
            if ((DateTime.Now - certDate).Days <= 60)
	        {
	            return "AboutToExpire";
	        }
            else if (certDate.AddYears(2) < DateTime.Now)
	        {
	            return "Expired";
	        }

	        return string.Empty;
	    }
	}

	public class CommentsDto {
		public DateTime ResponseDate { get; set; }
		public string SurveyResponse { get; set; }
		public string CaseNumber { get; set; }
	}
}