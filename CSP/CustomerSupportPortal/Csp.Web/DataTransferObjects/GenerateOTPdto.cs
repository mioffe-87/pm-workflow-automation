﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
 using System.Web.Mvc;
 using Csp.Api.Model;
 using Csp.Api.Model.Enums;

namespace Csp.Web.DataTransferObjects
{
    public class GenerateOTPdto : DefaultDto
    {
        public long SupportAccountID { get; set; }

        public IEnumerable<PanoramaSerialNumber> PanoramaSerialNumbers { get; set; }

        [Required]
        public string PanoramaSerialNumber { get; set; }

        [Required, Remote("ValidateDeviceForOtp", "Validation", AdditionalFields = "SupportAccountID")]
        public string SerialNumber { get; set; }

        public OTPOutput OtpOutput { get; set; }

        public string lookupCode { get; set; }

        public string productCategory { get; set; }

        public OtpDeviceType DeviceType { get; set; }

        public OtpType OtpType {get; set;}
    }
}