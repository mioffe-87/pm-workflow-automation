using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess.HelperClasses.Literals
{
    public class EndpointLicenseType
    {

        public const string EpSms = "TRAPSSM";
        public const string EpLabBundle = "LABB";
        public const string EpNfrBundle = "NFRB";
        public const string Ep100 = "TRAPSW";
        public const string Ep500 = "TRAPSS";
        public const string SvcEsm = "PREM";
        public const string SvcRenEsm = "RENPREM";
        public const string EpEval30 = "TRAPSEVAL30";
        public const string EpEval60 = "TRAPSEVAL60";
        public const string SvcEsmWrnt = "WRNT";
        public const string EpEval365 = "TRAPSEVAL365";
		public const string BKLNSvcEsm = "BKLN";
		public const string BKLNSvcRenEsm = "RENBKLN";
        public const string EpMobile = "TRAPSM";
        //public const string MalborkDbIdentificationPattern = "^(REN)*TRAPSMA(L|LUS|LLAB|LNFR)";
        public const string MalborkDbIdentificationPattern = "MAL|TRAPSELA";
        public const string MalborkSoftwareDownloadId = "TRAPSA";
        public const string MalborkSkuPattern = "^PAN-TRAPS-(([A-F]|BND-250-LAB|BND-250-NFR)-|AGENT-(BETA|E(15|30|60|365)))";
        public const string TrapsELA = "TRAPSELA";
        public const string Paa = "PAA";
        public const string PanoramaManagement = "M-P";
    }

    public class EndpointConstants
    {
        /*
        public const string StubPartId = "PAN-PA-EP-HW";
        public const string StubLabPartId = "PAN-PA-EP-LAB";
        public const string StubNfrPartId = "PAN-PA-EP-NFR";
        public const string StubEval30PartId = "PAN-PA-EP-200-50-E30";
        public const string StubEval60PartId = "PAN-PA-EP-200-50-E60";
        public const string StubEval365PartId = "PAN-PA-EP-200-50-E365";
         */
        public const string ModelCompat = "TRAPS";
				public const string Esm="1";
				public const string Workstation = "2";
				public const string Server = "3";
				public const string Support = "4";
				public const string Warranty = "5";
				public const string Vdi = "6";
				public const string SvcEsm = "PREM";
    }
}
