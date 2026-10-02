namespace Csp.Api.Model.LicenseManagement
{
    public class LicenseManagementActivatedListItem
    {
        public int UniqueId { get; set; }
        public string AuthCode { get; set; }

        public string SerialNumber { get; set; }

        public string ModelNumber { get; set; }

        public string SKU { get; set; }

        public string DeviceName { get; set; }

        public string ASC { get; set; }

        public string ExpirationDate { get; set; }

        public string ProductGroup { get; set; }
    }
}