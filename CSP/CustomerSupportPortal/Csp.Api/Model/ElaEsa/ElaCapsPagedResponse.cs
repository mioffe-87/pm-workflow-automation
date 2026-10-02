using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaCapsPagedResponse<T> : ElaCapsResponse<T>
    {
        [JsonProperty("currentPage")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PerPage { get; set; } = 10;

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
