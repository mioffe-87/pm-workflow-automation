using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class EnterpriseAgreementItem : SiteLicenseItem
    {
        public string EnterpriseAgreement { get; set; }
        public long SupportAccountId { get; set; }
        public string SupportAccountName { get; set; }
        public long CountOfDeviceEaIsAppliedTo { get; set; }
        public long TotalQuantity { get; set; }

        //shared with ESA/ELA/VM-ELA
        public string UsedTotalInfo
        {
            get
            {
                if (EnterpriseAgreement == "Traps-ELA")
                {
                    return TotalQuantity.ToString();
                }
                if (IsVmElaLicense)
                {
                    return string.Format("{0} / {1}", UsedLicenses.HasValue ? UsedLicenses.Value : 0, MaxLicenses.HasValue ? MaxLicenses.Value : 0);
                }
                else
                {
                    return CountOfDeviceEaIsAppliedTo.ToString();
                }
            }
        }

        //For VM-ELA, we need to hack Kendo's group to add the button
        public new string MainAuthCode
        {
            get
            {
                if (IsVmElaLicense)
                {
                    if (HasTokenManagementPermission)
                    {
                        return string.Format(@"{0} 
                            <div style=""float: right; width: 340px; margin-top: 5px;"">
                                <div style=""float: left; padding-left: 12px; width: 170px;"">{1} / {2}</div>   
                                <div style=""float: left; padding-left: 3px;"">{3}</div>   
                            </div>                     
                            <div style=""float: right; width: 60%; font-size: 85%"">
                                <input type=""button"" class=""k-button k-primary"" value=""Grant ELA Access"" onclick=""grantTokens('{0}')"">
                                <input type=""button"" class=""k-button k-primary"" value=""Manage VM/CN-X-Series Tokens"" onclick=""manageTokens('{0}')"" style=""margin-left:40px;"">
                            </div>
                            ", base.MainAuthCode, VmElaGrandParentUseTokens, VmElaGrandParentTotalTokens,
                            IsUnbounded ? "Unbounded" : "Bounded");
                    }
                    else
                    {
                        return string.Format(@"{0} 
                            <div style=""float: right; width: 340px; margin-top: 5px;"">
                                <div style=""float: left; padding-left: 12px; width: 170px;"">{1} / {2}</div>   
                                <div style=""float: left; padding-left: 3px;"">{3}</div>   
                            </div>                     
                            ", base.MainAuthCode, VmElaGrandParentUseTokens, VmElaGrandParentTotalTokens,
                            IsUnbounded ? "Unbounded" : "Bounded");                        
                    }
                }
                else
                {
                    return base.MainAuthCode;
                }
            }
            set { base.MainAuthCode = value; }
        }

        //VM-ELA use
        public bool IsVmElaLicense { get; set; }
        public bool IsUnbounded { get; set; }
        public long VmElaGrandParentTotalTokens { get; set; }
        public long VmElaGrandParentUseTokens { get; set; }
        public string BoundedOrUnbounded { get; set; }
        public bool HasTokenManagementPermission { get; set; }
    }
}
