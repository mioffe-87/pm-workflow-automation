using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model
{
    public class TokenCalculatorServiceResponse
    {
        [JsonProperty("totalTokens", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double TotalTokens { get; set; }

        [JsonProperty("tokenSummary", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TokenCalculatorSummary TokenSummary { get; set; }
    }
    public class TokenCalculatorResponse
    {
        [JsonProperty("totalTokens", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double TotalTokens { get; set; }

        [JsonProperty("tokenSummary", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TokenCalculatorSummary TokenSummary { get; set; }
    }

    public class TokenCalculatorSummary
    {
        [JsonProperty("support", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Support { get; set; }

        [JsonProperty("firewall", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Firewall { get; set; }

        [JsonProperty("firewallSupport", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double FirewallSupport { get; set; }

        [JsonProperty("panorama", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Panorama panorama { get; set; }

        [JsonProperty("panoramaSupport", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double PanoramaSupport { get; set; }

        [JsonProperty("subscription", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TokenCalculatorSubscription Subscription { get; set; }
    }


    public class TokenCalculatorSubscription
    {
        [JsonProperty("TP", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double TP { get; set; }

        [JsonProperty("URL4", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double URL4 { get; set; }

        [JsonProperty("DNS", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double DNS { get; set; }

        [JsonProperty("WF", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double WF { get; set; }

        [JsonProperty("GP", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double GP { get; set; }

        [JsonProperty("DLP", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double DLP { get; set; }

        [JsonProperty("SDWAN", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double SDWAN { get; set; }

        [JsonProperty("ASDWAN", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double ASDWAN { get; set; }

        [JsonProperty("AURL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double AURL { get; set; }

        [JsonProperty("ITO", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double ITO { get; set; }

        [JsonProperty("ATP", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double ATP { get; set; }

        [JsonProperty("HSF", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double HSF { get; set; }
        
        [JsonProperty("WP", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double WP { get; set; }

        [JsonProperty("AWF", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double AWF { get; set; }

        [JsonProperty("ADNS", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double ADNS { get; set; }

        [JsonProperty("ENIOTDRDL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double ENIOTDRDL { get; set; }

        [JsonProperty("ENIOTPDRDL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double ENIOTPDRDL { get; set; }

        [JsonProperty("INDIOTDRDL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double INDIOTDRDL { get; set; }

        [JsonProperty("MDIOTDRDL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MDIOTDRDL { get; set; }

        [JsonProperty("ENIOTPRDL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double ENIOTPRDL { get; set; }

        [JsonProperty("INDIOTRDL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double INDIOTRDL { get; set; }

        [JsonProperty("MDIOTRDL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MDIOTRDL { get; set; }

        [JsonProperty("AIOPS", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double AIOPS { get; set; }

        [JsonProperty("SAASINLINE", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double SAASINLINE { get; set; }

        [JsonProperty("CIE", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double CIE { get; set; }

        [JsonProperty("SCM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double SCM { get; set; }

        [JsonProperty("SLS", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double SLS { get; set; }

        [JsonProperty("MSF", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MSF { get; set; }

        [JsonProperty("AIPD", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double AIPD { get; set; }
    }

    public class Panorama
    {
        public double MANAGEMENT { get; set; }
        public double LOGCOLLECTOR { get; set; }
    }
}
 
