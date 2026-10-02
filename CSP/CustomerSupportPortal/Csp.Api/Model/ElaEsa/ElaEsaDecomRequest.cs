using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    /// <summary>
    /// Request object of the Get Decom History POST end-point.
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
    public class ElaEsaDecomRequest
    {
        [JsonProperty("params")]
        public ElaEsaDecomRequestParams Params { get; set; }

        [JsonProperty("data")]
        public ElaEsaDecomRequestData Data { get; set; }
    }
}
