using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Security;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    /// <summary>
    /// CSP-2229 RBAC role: License Transfer
    /// RBAC user role object from hub
    /// </summary>
    /// E.g. [
    ///   {"email":"dsen@paloaltonetworks.com","csp_account_id":"245","type":"special_predefined","role":"account_superuser"},
    ///   {"email":"mchan@paloaltonetworks.com","csp_account_id":"123","type":"app_predefined","app":"xdr","role":"app_superuser","tenant_id":"*"}]
    /// ]
    public class RbackUserRole
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("csp_account_id")]
        public string SupportAccountId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("app")]
        public string Application { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("tenant_id")]
        public string TenantId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public static bool operator ==(RbackUserRole role1, RbackUserRole role2)
        {
            return string.Compare(role1.Application, role2.Application, true) == 0
                && string.Compare(role1.Email, role2.Email, true) == 0
                && string.Compare(role1.Role, role2.Role, true) == 0
                && string.Compare(role1.TenantId, role2.TenantId, true) == 0
                && string.Compare(role1.Type, role2.Type, true) == 0
                && string.Compare(role1.Version, role2.Version, true) == 0;
        }

        public static bool operator !=(RbackUserRole role1, RbackUserRole role2)
        {
            return !(role1 == role2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Email)
                    && !string.IsNullOrWhiteSpace(SupportAccountId)
                    && !string.IsNullOrWhiteSpace(Type)
                    && !string.IsNullOrWhiteSpace(Role);
            }
        }
    }
}
