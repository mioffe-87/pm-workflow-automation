using Newtonsoft.Json;
using System;

namespace Csp.Web.DataTransferObjects
{
    public class LicensingApiDto : DefaultDto
    {
        public LicensingApiAction LicensingApiAction { get; set; }
        public long LicensingApiKey { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool HasAccessToManageKey { get; set; }
        public long SupportAccountId { get; set; }
        public long UserAccountId { get; set; }
        public int? ProductId { get; set; }
        public int? ApiKeyId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }


    public enum LicensingApiAction
    {
        ENABLE,
        EXTEND,
        DISABLE,
        REGENERATE
    }

    
}