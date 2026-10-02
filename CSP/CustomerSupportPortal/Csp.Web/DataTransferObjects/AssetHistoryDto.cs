using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class AssetHistoryDto : DefaultDto
    {
        public long SupportAccountId { get; set; }

        public AssetHistoryModel AssetHistory { get; set; }
    }
}