using System;
using System.Collections.Generic;

namespace Csp.Api.Model
{
    public class VmRenewalStatus {
        public const string StatusActive = "Active";
        public const string StatusRenewalPending = "You have selected renewal";
        public const string StatusRenewalDowngradedPending = "You have selected change to Basic Bundle";
        public const string StatusRenewalForfeitedPending = "You have selected Forfeit";
        public const string StatusDowngradedPending = "Basic Bundle pending completion of renewal process";
        public const string StatusForfeitedPending = "Forfeit pending completion of renewal process";
        public const string StatusDowngraded = "Changed to Basic Bundle";
        public const string StatusForfeited = "Forfeited";    
    }

    public class VmRenewalDevice
    {
        public string SerialNumber { get; set; }
        public bool IsSelected { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsDowngraded { get; set; }
        public bool IsForfeited { get; set; }
        public string Status { get; set; }

        public VmRenewalDevice()
        {
            this.Status = VmRenewalStatus.StatusActive;
        }
    }

    public class VmRenewalUnprovisionedInfo
    {
        public int RenewalQuantity { get; set; }
        public int DowngradeQuantity { get; set; }
        public int ForfeitQuantity { get; set; }
    }

    public class VmRenewalModel
    {
        public IEnumerable<VmRenewalDevice> RenewalDevices { get; set; }
        public VmRenewalUnprovisionedInfo UnprovisionedInfo { get; set; }
        public string AuthCode { get; set; }
        public string Description { get; set; }
        public string LatestQuoteNumber { get; set; }
        public bool SelectAll { get; set; }
        public int TotalQuantity { get; set; }
        public int ProvisionedQuantity { get; set; }
        public string PartNumber { get; set; }
        public bool LockUi { get; set; }
        public bool HasPendingRenewals { get; set; }

        public VmRenewalModel()
        {
            RenewalDevices = new List<VmRenewalDevice>();
        }
    }
}
