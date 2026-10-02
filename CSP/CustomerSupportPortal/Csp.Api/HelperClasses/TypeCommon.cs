using System;
using System.Configuration;
using System.Net;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.IO;
using Elmah;

namespace Csp.Api.HelperClasses 
{
    public static class TypeCommon
    {
        private static int GetNumberOfFullYears(DateTime start, DateTime end, int years)
        {
            return start.AddYears(years + 1) > end
                ? years
                : GetNumberOfFullYears(start, end, years + 1);
        }

        private static int GetNumberOfFullMonths(DateTime start, DateTime end, int months)
        {
            return start.AddMonths(months + 1) > end
                ? months
                : GetNumberOfFullMonths(start, end, months + 1);
        }

        public static string TimeSpanToYearsMonthsDays(DateTime startDate, DateTime endDate, string emptyMessage = "None")
        {
            DateTime start = startDate.Date;
            DateTime end = endDate.Date;

            if (start > end)
            {
                return emptyMessage;
            }

            int years = GetNumberOfFullYears(start, end, 0);
            int months = GetNumberOfFullMonths(start.AddYears(years), end, 0);
            int days = (int)(endDate - start.AddYears(years).AddMonths(months)).TotalDays;

            return years == 0 && months == 0 && days == 0
                ? emptyMessage
                : ((years > 0 ? string.Format("{0} year{1}", years, years == 1 ? string.Empty : "s") : string.Empty)
                    + (months > 0 ? string.Format(" {0} month{1}", months, months == 1 ? string.Empty : "s") : string.Empty)
                    + (days > 0 ? string.Format(" {0} day{1}", days, days == 1 ? string.Empty : "s") : string.Empty)).Replace("  ", " ").Trim();
        }
    }
}

