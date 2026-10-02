using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using ExpressiveAnnotations.Attributes;

namespace Csp.Web.DataTransferObjects
{
    public class DeviceTypeDto : DefaultDto
    {
        public long SupportAccountId { get; set; }
        public string Email { get; set; }

        public int DeviceType { get; set; }

        public int CloudType { get; set; }

        public IEnumerable<CloudType> CloudTypes { get; set; }

    }

    enum DeviceTypes
    {
        PanDevice = 1, 
        CloudDevice = 2,
        PartialShipment = 3
    };
}