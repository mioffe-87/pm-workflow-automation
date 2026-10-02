using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using System;

namespace Csp.Api.Model.LicenseManagement
{
    public class LicenseManagementAvailableListItem
    {
        public long UniqueId { get; set; }

        public string AuthCode { get; set; }

        public string Description { get; set; }

        public string SKU { get; set; }

        public int LicenseTermMonths { get; set; }

        public int GracePeriodDays { get; set; }

        public DateTime? ShipDate { get; set; }

        public string OriginalTerm
        {
            get
            {
                var startDate = (RegistrationDate ?? DateTime.Now).Date;
                return LicenseTermMonths > 0
                    ? TypeCommon.TimeSpanToYearsMonthsDays(startDate, startDate.AddMonths(LicenseTermMonths), "N/A") // months
                    : TypeCommon.TimeSpanToYearsMonthsDays(startDate, startDate.AddDays(GracePeriodDays), "N/A"); // days
            }
        }

        public string DaysLeft
        {
            get
            {
                if (RegistrationDate.HasValue)
                {
                    var startDate = RegistrationDate;
                    if (!startDate.HasValue)
                    {
                        return OriginalTerm; // license has not been ever allocated
                    }
                    var endDate = LicenseTermMonths > 0
                        ? startDate.Value.AddMonths(LicenseTermMonths) // months
                        : startDate.Value.AddDays(GracePeriodDays); // days
                    var now = DateTime.Now.Date;
                    return now > endDate
                        ? "None"
                        : LicenseTermMonths > 0
                            ? TypeCommon.TimeSpanToYearsMonthsDays(now, startDate.Value.AddMonths(LicenseTermMonths), "N/A") // months
                            : TypeCommon.TimeSpanToYearsMonthsDays(now, startDate.Value.AddDays(GracePeriodDays), "N/A"); // days
                }
                else
                {
                    var startDate = DateTime.Now.Date;
                    return LicenseTermMonths > 0
                            ? TypeCommon.TimeSpanToYearsMonthsDays(startDate, startDate.AddMonths(LicenseTermMonths), "N/A") // months
                            : TypeCommon.TimeSpanToYearsMonthsDays(startDate, startDate.AddDays(GracePeriodDays), "N/A"); // days
                }    
            }
        }

        public DateTime? RegistrationDate { get; set; }        

        public string Status { get; set; }

        public int LicenseType { get; set; }

        public string ProductGroup { get; set; }

        [JsonIgnore]
        public string Hash { get; set; }

        public string ActivationLink { get; set; }
    }
}
