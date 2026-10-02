namespace Csp.Api.Model
{
    public class RmaDepot
    {
        public int RmaDepotId { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public bool Active { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string WarehouseName { get; set; }
        public bool X4HRDepot { get; set; }
        public string ZipCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

    }

    public class DepotDistance
    {
        public RmaDepot Depot { get; set; }
        public long DepotId { get; set; }
        public string DepotName { get; set; }
        public string DepotCountry { get; set; }
        public string DistanceText { get; set; }
        public int DistanceMeters { get; set; }
        public string DurationText { get; set; }
        public int DurationSeconds { get; set; }
        public bool IsCalculated { get; set; }
        public string ErrorReason { get; set; }
    }
}