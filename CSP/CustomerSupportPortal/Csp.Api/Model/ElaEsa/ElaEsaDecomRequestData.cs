using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    /// <summary>
    /// <code>
    /// params: {
    ///     userAccountId,
    ///     supportAccountId,
    /// },
    /// data: {
    ///     currentPage: 1,
    ///     pageSize: 10,
    ///     sortOrder: 'ASC',
    ///     sortField: 'modal',
    ///     onlyForDecommission: true,
    /// },
    /// </code>
    /// </summary>
    public class ElaEsaDecomRequestData
    {
        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("sortOrder")]
        public string SortOrder { get; set; }

        [JsonProperty("sortField")]
        public string SortField { get; set; }
    }
}
