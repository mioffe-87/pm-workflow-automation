using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum FwFlexRequestType
    {
        [Description("ACTIVATE")]
        TokenActivate,

        [Description("SUMMARY")]
        TokenSummary,

        [Description("ALLOCATE")]
        TokenAllocate,

        [Description("CONSUME")]
        TokenConsume,

        [Description("TRANSFER")]
        TokenTransfer,

        [Description("PROVISION")]
        Provision,
        
        [Description("LICENSING")]
        Licensing,

        [Description("MIGRATEPANORAMA")]
        MigratePanorama,

        [Description("DELETE")]
        Delete,

        [Description("DELETEPANORAMA")]
        DeletePanorama,

        [Description("RENAME_POOL")]
        RenamePool,

       [Description("TRANSFER_ALLOCATE")]
        TransferAllocate

    }
}
