using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Csp.Web.DataTransferObjects
{
    public class SsoConfigurationDto : DefaultDto
    {
				public string ThirdPartySsoConfigUrl { get; set; }
        public string Domain { get; set; }
    }
}
