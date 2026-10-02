using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class AssetHistoryModel
    {
        public AssetHistorySearchModel SearchModel { get; set; }
        public IEnumerable<AssetHistoryListItem> Assets { get; set; }
    }

    public class AssetHistorySearchModel
    {
        public AssetHistoryFilterTypes AssetHistoryFilterType { get; set; }
        public string Keyword { get; set; }
        public string GroupBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public enum AssetHistoryFilterTypes
    {
        Code,
        DateRange
    }

    public class AssetHistoryListItem
    {
        public long Id { get; set; }
        public string AssetType { get; set; }
        public string AssetTypeDisplayName { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime? DateRegistered { get; set; }
        public DateTime? DateShipped { get; set; }
        public DateTime? DatePurchased { get; set; }
        public long SupportAccountId { get; set; }
        public string SupportAccountName { get; set; }
        public bool HasMembership { get; set; }
        public long PartMasterId { get; set; }
        public long? PartSubtypeId { get; set; }
        public string ModelCompatibility { get; set; }
        public string PanDbIdentification { get; set; }

        public long? RegisteredByUserId { get; set; }
        public string RegisteredByEmail { get; set; }
        public string RegisteredByName { get; set; }
        public long? RegisteredAccountId { get; set; }
        public string RegisteredAccountName { get; set; }

        public List<AssetHistorySpareLicenseTransferDetails> SpareLicenseTransferDetails { get; set; }
    }

    public class AssetHistoryDeviceRegDetails
    {
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public long? RegisteredByUserId { get; set; }
        public string RegisteredByEmail { get; set; }
        public string RegisteredByName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }

    public class AssetHistorySpareLicenseTransferDetails
    {
        public long DeviceId { get; set; }
        public bool IsTransferred { get; set; }
        public string TransferStatus
        {
            get
            {
                return (IsTransferred) ? "Success" : "Failed";
            }
        }
        public long? TransferredByUserId { get; set; }
        public string TransferredByEmail { get; set; }
        public string TransferredByName { get; set; }
        public DateTime? DateTransferred { get; set; }
        public string SourceSerialNumber { get; set; }
        public string DestinationSerialNumber { get; set; }
    }
}
