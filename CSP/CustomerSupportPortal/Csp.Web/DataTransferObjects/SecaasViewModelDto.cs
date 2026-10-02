using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class SecaasViewModelDto:DefaultDto
    {
        public long SupportAccountId { get; set; }
        public string DefaultAuthCode { get; set; }
        public IEnumerable<AvailableSaaSActivations> AvailableSaas { get; set; }
        public bool hasOTPAccess { get; set; }
    }
}