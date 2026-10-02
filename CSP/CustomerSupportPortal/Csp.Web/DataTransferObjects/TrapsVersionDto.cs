using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class TrapsVersionDto : DefaultDto
    {
        public string AuthorizationCode { get; set; }

        public string CurrentVersionNumber { get; set; }

        public int? CurrentWorkstationUsecount { get; set; }
        public int? CurrentServerUsecount { get; set; }
        public int? CurrentVdiUsecount { get; set; }

        public int? WorkstationUsecount { get; set; }
        public int? ServerUsecount { get; set; }
        public int? VdiUsecount { get; set; }
        public string VersionNumber { get; set; }

        public int TotalUsecount { get; set; }

    }
}