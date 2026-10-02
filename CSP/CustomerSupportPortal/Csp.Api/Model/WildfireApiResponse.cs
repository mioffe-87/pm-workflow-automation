using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class WildfireApiResponse
    {
        public long supportAccountId { get; set; }

        public string apiKey { get; set; }

        public string serialNumber { get; set; }

        public string productCode { get; set; }

        public string productType { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public bool isActive { get; set; }

        public string status { get; set; }

        public int queryMaxLimit { get; set; }

        public int uploadMaxLimit { get; set; }

        public string endDateString { get; set; }

        public DateTime createdDate { get; set; }

        public DateTime updatedDate { get; set; }
    }

    public class ResponseWildfireKey
    {
        public WildfireApiResponse data { get; set; }
    }

    /// <summary>
    /// class for array response
    /// </summary>
    public class ResponseWildfireKeys
    {
        public int totalPages { get; set; }
        public long totalElements { get; set; }
        public List<WildfireApiResponse> wildfireAPIkeys { get; set; }
    }

    public class ResponseWildfireKeyList
    {
        public ResponseWildfireKeys data { get; set; }
    }
}
