using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class ChassisComponentDto:DefaultDto
    {
        public ChassisComponent ChassisComponent { get; set; }

        public EulaModel Eula { get; set; }

    }
}