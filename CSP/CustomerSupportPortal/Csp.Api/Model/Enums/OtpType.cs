using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum OtpType
    {
        [Description("LoggingService")]
        LoggingService,

        [Description("ZTPService")]
        ZtpService,

        [Description("PanOS")]
        PanOS,

        [Description("Panorama")]
        Panorama,
    }

    public enum OtpDeviceType
    {
        [Description("PanOS")]
        PanOS =1,
        [Description("Panorama")]
        Panorama =2,
        [Description("Panorama Managed")]
        BulkPanOS =3,
        [Description("Logging Service")]
        PanoramaPrismaAccess = 4,
        [Description("Wf500")]
        Wf500 = 5
    }

    public enum ProcessingStatus
    {
        None = 0,
        Pending = 1,
        Complete = 2,
        Failed = 3
    }

}
