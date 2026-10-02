using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class RequestParameters
    {
        [JsonProperty("page")]
        int Page { get; set; }

        [JsonProperty("results")]
        int PageSize { get; set; }

        [JsonProperty("sortField")]
        string SortField { get; set; }

        [JsonProperty("sortOrder")]
        SortDirection SortOrder { get; set; }

        public RequestParameters()
        {
            Page = 1;
            PageSize = int.MaxValue;
        }

        public static DataParameters ToDataParameter(RequestParameters request)
        {
            DataParameters dataParameters = new DataParameters();
            if (request == null)
                request = new RequestParameters();

            int page = 1;
            int pageSize = 10;

            dataParameters.PaginationCount = request.PageSize == 0 ? pageSize : request.PageSize;
            dataParameters.PaginationOffset = request.Page == 0 ? page : request.Page;
            dataParameters.SortField = request.SortField;
            dataParameters.SortDirection = request.SortOrder;

            return dataParameters;
        }

    }
}
