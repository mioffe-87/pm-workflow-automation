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
    /// Class to aggregate all required properties for transfer of RBAC role from one Support Account to another one.
    /// Definitions: 
    ///  - 'the SN' means the Searial Number, which is being transfered.
    ///  - 'TID' - tenant ID
    ///  - 'version' and 'app' are not taken into consideration because SN (Tenant) is not app and version sensitive
    ///  - tenant defined - '*' or number, not NULL 
    ///  - tenant specified value - number, not NULL and not '*'
    /// </summary>
    public class RbackSupportAccountSnTenantTransferProperties
    {
        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Source Suppord Account ID
        /// </summary>
        public string SupportAccountId { get; set; }
        /// <summary>
        /// Serial number to transfer
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// Tenant ID of the Serial Number
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// Support Account has: 
        /// TRUE - the SN, 
        /// FALSE - no matching the SN
        /// </summary>
        public bool HasSn { get; set; }
        /// <summary>
        /// Support Account has: 
        /// TRUE - TID matching the SN, 
        /// FALSE - no TID matching the SN
        /// </summary>
        public bool HasMatchingTenant { get; set; }
        /// <summary>
        /// Support Account has: 
        /// TRUE - other SN(s) with matching the TID => no role-tenant delete, 
        /// FALSE - no other SN with matching the TID - check role-tenant delete
        /// </summary>
        public bool HasOtherSnMatchingTenant { get; set; }
        /// <summary>
        /// Support Account has: 
        /// TRUE - at least 1 SN which does not match the TID => no role delete, 
        /// FALSE - no other SN with no matching the TID - check role delete
        /// </summary>
        public bool HasOtherSnNotMatchingTenant { get; set; }
    }
}
