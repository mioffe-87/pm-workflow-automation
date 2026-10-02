using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum FwFlexCreditType
    {
        [Description("PAN-PRISMA-NGFW-LAB")]
        PRISMA_LAB,

        [Description("PAN-PRISMA-NGFW-NFR")]
        PRISMA_NFR,

        [Description("PAN-PRISMA-NGFW-CR")]
        PRISMA_PROD,

        [Description("PAN-PRISMA-NGFW-CR")]
        PRISMA_EVAL,

        [Description("PAN-PRISMA-NGFW-NFR")]
        PRISMA_NFR_EVAL,

        [Description("PAN-PRISMA-NGFW-CR-SE")]
        PRISMA_SE_EVAL,

        [Description("PAN-VIRTUAL-NGFW-LAB")]
        VIRTUAL_LAB,

        [Description("PAN-VIRTUAL-NGFW-NFR")]
        VIRTUAL_NFR,

        [Description("PAN-VIRTUAL-NGFW-CR")]
        VIRTUAL_PROD,

        [Description("PAN-VIRTUAL-NGFW-CR")]
        VIRTUAL_EVAL,

        [Description("PAN-VIRTUAL-NGFW-NFR")]
        VIRTUAL_NFR_EVAL,

        [Description("PAN-VIRTUAL-NGFW-CR-SE")]
        VIRTUAL_SE_EVAL,

        [Description("PAN-SOFTWARE-NGFW-CR")]
        SOFTWARE_PROD,

        [Description("PAN-SOFTWARE-NGFW-CR")]
        SOFTWARE_EVAL,

        [Description("PAN-SOFTWARE-NGFW-LAB")]
        SOFTWARE_LAB,

        [Description("PAN-SOFTWARE-NGFW-NFR")]
        SOFTWARE_NFR,

        [Description("PAN-SOFTWARE-NGFW-CR-SE")]
        SOFTWARE_SE_EVAL,

        [Description("PAN-SOFTWARE-NGFW-SUB-CR")]
        SUBSCRIPTION_PROD,

        [Description("PAN-SOFTWARE-NGFW-SUB-LAB")]
        SUBSCRIPTION_LAB,

        [Description("PAN-SOFTWARE-NGFW-SUB-NFR")]
        SUBSCRIPTION_NFR,

        [Description("PAN-SOFTWARE-NGFW-SUB-CR-SE")]
        SUBSCRIPTION_SE_EVAL,

        [Description("PAN-SOFTWARE-NGFW-CR-MSSP")]
        MSSP_PROD, 

        [Description("PAN-CLOUD-NGFW-CR")]
        CLOUD_NGFW_CR
    }


    public enum CreditPoolGenType
    {
        [Description("GEN-1")]
        Gen1,

        [Description("GEN-2")]
        Gen2
    }
}
