using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class AllocateCnSeriesTokensToPanoramaDto 
    {
        public long Id { get; set; }

        [Required]
        public string SerialNumber { get; set; }

        public string ModelName { get; set; }

        [Required]
        public string CnxAuthCode { get; set; }

        [Required]
        public int Tokens { get; set; }
    }
}
