using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class VmModelUpgradeDto : DefaultDto
    {
        public bool IsValidUpgradeAuthCode { get; set; }

        public string FromModel { get; set; }

        public string ToModel { get; set; }

        public string SerialNumber { get; set; }

        public string UpgradeMessage { get; set; }

    }
}