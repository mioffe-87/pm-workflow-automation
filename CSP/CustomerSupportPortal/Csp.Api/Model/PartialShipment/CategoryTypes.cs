using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace Csp.Api.Model.PartialShipment
{
    enum ShipmentTypes
    {
        Device,
        Support,
        Subscription,
        AccessoryAndOtherAddOns
    }
    enum CategoryTypes
    {
        DEV,
        RENSUB,
        RENSUP,
        SUB,
        SUP
    }
    /// <summary>
    /// Clarity Components -Part SubType is added
    /// </summary>
    enum SubscriptionTypes
    {
        [Description("Data Subscription")]
        CGX_BW,
        [Description("Data Subscription")]
        CGX_DC,
        [Description("Data Subscription")]
        CGX_SUB_BR,
        [Description("Clarity Reporting Subscription")]
        CGX_WANR,        
        [Description("Clarity NDVR Subscription")]
        CGX_NDVR,
        [Description("Zone Based Firewall Subscription")]
        CGX_ZBFW
    }
}
