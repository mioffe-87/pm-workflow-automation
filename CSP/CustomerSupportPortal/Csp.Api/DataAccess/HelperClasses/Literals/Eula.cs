using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess.HelperClasses.Literals
{
    public class EulaLanguage
    {
        public const int English = 1;
        public const int Japanese = 2;
    }

    public class EulaContext
    {
        public const int RegisterDevice = 1;
        public const int ActivateLicense = 2;
        public const int ActivateVmAuthCode = 3;
        public const int ActivateEndpointAuthCode = 4;
        public const int AcceptDeviceTransfer = 5;
        public const int BulkRegistration = 6;
        public const int ActivateCnSeriesAuthCode = 7;
    }

    public enum DeviceType
    {
        AZURE =2,
        GCP =5,
        AWS = 1,
        CSSP = 3,
        NGFW=0,
        CLOUDAZURE = 10,
        CLOUDAWS = 11,
        CLOUDAIAWS = 12, 
        ROUTE53 = 13
    }

}
