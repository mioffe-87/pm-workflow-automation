using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class AddEpLicensesDto : DefaultDto
    {
        public long? DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string SerialNumber { get; set; }

        [Required]
        public string NewAuthCode { get; set; }


        public string EsmAuthCode { get; set; }
        public IEnumerable<EpLicenses> Licenses { get; set; }
        public bool IsNewEndpointActivation { get; set; }

        public EulaModel Eula { get; set; }

    }
}