using System;
using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class LicenseData {

		private const string MySqlDateFormat = "yyyy-MM-dd HH:mm:ss";

        //SELECT CONCAT('new license { LicenseId = ', l.LicenseId, ', PartMasterId = ', l.PartMasterId, ', SupportAccountId = ', l.SupportAccountId, ', SupportCenterSupportAccountId = ', IF(ISNULL(l.SupportCenterSupportAccountId), 'null', CONCAT('"', l.SupportCenterSupportAccountId, '"')), ', DeviceId = ', l.DeviceId, ', ExpirationDate = DateTime.Parse("', l.ExpirationDate, '"), IsActive = ', IF(l.IsActive = 1, 'true', 'false'), ', GracePeriodDays = ', l.GracePeriodDays, ', LicenseTermMonths = ', l.LicenseTermMonths, ', ShipDate = DateTime.Parse("', l.ShipDate, '"), ShipmentNumber = ', l.ShipmentNumber, ' },')

        #region Basic Seed
        public static readonly license[] PanLicensesBasic = new license[]
      {
        new license { LicenseId = 84, PartMasterId = 2, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 2, ExpirationDate = DateTime.Parse("2008-03-08 14:52:33"), IsActive = true, GracePeriodDays = 30, LicenseTermMonths = 12, ShipDate = DateTime.Parse("2007-03-02 00:00:00"), ShipmentNumber = 0 },
        new license { LicenseId = 182, PartMasterId = 15, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 2, ExpirationDate = DateTime.Parse("2007-07-13 12:08:08"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 1, ShipDate = DateTime.Parse("2007-06-13 12:07:13"), ShipmentNumber = 0 },
        new license { LicenseId = 229, PartMasterId = 17, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 2, ExpirationDate = DateTime.Parse("2007-10-02 14:20:24"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 2, ShipDate = DateTime.Parse("2007-08-02 14:20:14"), ShipmentNumber = 0 },
        new license { LicenseId = 817656, PartMasterId = 3262, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 2, ExpirationDate = DateTime.Parse("2014-02-15 15:47:14"), IsActive = true, GracePeriodDays = 30, LicenseTermMonths = 0, ShipDate = DateTime.Parse("2014-01-16 15:47:14"), ShipmentNumber = 0 },
        new license { LicenseId = 120, PartMasterId = 2, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2007-06-19 00:00:00"), IsActive = false, GracePeriodDays = 30, LicenseTermMonths = 2, ShipDate = DateTime.Parse("2007-03-20 00:00:00"), ShipmentNumber = 27 },
        new license { LicenseId = 6260, PartMasterId = 222, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2010-03-23 13:02:01"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 12, ShipDate = DateTime.Parse("2009-03-23 13:02:01"), ShipmentNumber = 0 },
        new license { LicenseId = 6301, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2010-09-28 08:36:56"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2009-03-23 16:36:54"), ShipmentNumber = 0 },
        new license { LicenseId = 6302, PartMasterId = 39, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2011-03-23 16:37:47"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 24, ShipDate = DateTime.Parse("2009-03-23 16:37:47"), ShipmentNumber = 0 },
        new license { LicenseId = 24112, PartMasterId = 226, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2013-03-23 16:07:55"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2010-03-23 16:07:55"), ShipmentNumber = 0 },
        new license { LicenseId = 30430, PartMasterId = 11, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2010-08-01 00:00:00"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 3, ShipDate = DateTime.Parse("2010-06-01 11:29:52"), ShipmentNumber = 0 },
        new license { LicenseId = 60011, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2013-09-28 08:36:56"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2011-01-10 09:25:33"), ShipmentNumber = 0 },
        new license { LicenseId = 76743, PartMasterId = 39, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2014-03-23 16:37:47"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2011-03-23 08:32:32"), ShipmentNumber = 0 },
        new license { LicenseId = 356633, PartMasterId = 2818, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2015-11-16 13:24:32"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-11-16 13:24:32"), ShipmentNumber = 0 },
        new license { LicenseId = 372469, PartMasterId = 1433, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2015-11-29 10:55:39"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-11-29 10:55:39"), ShipmentNumber = 0 },
        new license { LicenseId = 375739, PartMasterId = 344, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2017-03-23 16:37:47"), IsActive = true, GracePeriodDays = 30, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-12-04 00:00:00"), ShipmentNumber = 0 },
        new license { LicenseId = 474596, PartMasterId = 994, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2013-04-17 12:55:34"), IsActive = true, GracePeriodDays = 30, LicenseTermMonths = 0, ShipDate = DateTime.Parse("2013-03-18 12:55:34"), ShipmentNumber = 0 },
        new license { LicenseId = 665396, PartMasterId = 995, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2013-10-05 13:27:08"), IsActive = true, GracePeriodDays = 30, LicenseTermMonths = 0, ShipDate = DateTime.Parse("2013-09-05 13:27:08"), ShipmentNumber = 0 },
        new license { LicenseId = 695228, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 11, ExpirationDate = DateTime.Parse("2017-11-28 08:36:56"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 50, ShipDate = DateTime.Parse("2013-10-02 08:30:59"), ShipmentNumber = 0 },
        new license { LicenseId = 140, PartMasterId = 2, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2007-07-02 11:51:47"), IsActive = false, GracePeriodDays = 30, LicenseTermMonths = 2, ShipDate = DateTime.Parse("2007-04-03 00:00:00"), ShipmentNumber = 37 },
        new license { LicenseId = 141, PartMasterId = 11, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2007-08-02 11:51:47"), IsActive = false, GracePeriodDays = 30, LicenseTermMonths = 3, ShipDate = DateTime.Parse("2007-04-03 00:00:00"), ShipmentNumber = 37 },
        new license { LicenseId = 142, PartMasterId = 1824, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2007-07-02 12:08:15"), IsActive = false, GracePeriodDays = 30, LicenseTermMonths = 2, ShipDate = DateTime.Parse("2007-04-03 00:00:00"), ShipmentNumber = 38 },
        new license { LicenseId = 1643, PartMasterId = 39, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2009-05-19 08:45:49"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 12, ShipDate = DateTime.Parse("2008-05-19 08:45:49"), ShipmentNumber = 0 },
        new license { LicenseId = 4160, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2009-11-27 14:23:11"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 12, ShipDate = DateTime.Parse("2008-12-17 08:50:33"), ShipmentNumber = 0 },
        new license { LicenseId = 9730, PartMasterId = 344, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2012-07-27 13:58:18"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2009-07-27 13:58:18"), ShipmentNumber = 0 },
        new license { LicenseId = 9731, PartMasterId = 339, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2012-07-27 13:59:14"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2009-07-27 13:59:14"), ShipmentNumber = 0 },
        new license { LicenseId = 19234, PartMasterId = 324, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2012-11-27 14:23:11"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2010-01-25 15:36:09"), ShipmentNumber = 0 },
        new license { LicenseId = 298648, PartMasterId = 344, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2015-08-08 13:19:15"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-08-08 13:19:15"), ShipmentNumber = 0 },
        new license { LicenseId = 298649, PartMasterId = 339, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2015-08-08 13:20:59"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-08-08 13:20:59"), ShipmentNumber = 0 },
        new license { LicenseId = 298650, PartMasterId = 316, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2015-11-27 14:23:11"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-08-08 13:25:42"), ShipmentNumber = 0 },
        new license { LicenseId = 356630, PartMasterId = 2818, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2015-11-16 13:13:36"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-11-16 13:13:36"), ShipmentNumber = 0 },
        new license { LicenseId = 372468, PartMasterId = 1433, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 18, ExpirationDate = DateTime.Parse("2015-11-29 10:52:34"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-11-29 10:52:34"), ShipmentNumber = 0 },
        new license { LicenseId = 179, PartMasterId = 2, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 25, ExpirationDate = DateTime.Parse("2007-07-11 00:00:00"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 1, ShipDate = DateTime.Parse("2007-06-11 00:00:00"), ShipmentNumber = 0 },
        new license { LicenseId = 180, PartMasterId = 1824, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 25, ExpirationDate = DateTime.Parse("2007-07-11 00:00:00"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 1, ShipDate = DateTime.Parse("2007-06-11 00:00:00"), ShipmentNumber = 0 },
        new license { LicenseId = 181, PartMasterId = 13, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 25, ExpirationDate = DateTime.Parse("2007-07-11 00:00:00"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 1, ShipDate = DateTime.Parse("2007-06-11 00:00:00"), ShipmentNumber = 0 },
        new license { LicenseId = 192719, PartMasterId = 344, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 25, ExpirationDate = DateTime.Parse("2015-01-30 15:56:49"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-01-30 15:56:49"), ShipmentNumber = 0 },
        new license { LicenseId = 192726, PartMasterId = 324, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 25, ExpirationDate = DateTime.Parse("2010-07-11 00:00:00"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-01-30 15:59:43"), ShipmentNumber = 0 },
        new license { LicenseId = 192739, PartMasterId = 324, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 25, ExpirationDate = DateTime.Parse("2013-07-11 00:00:00"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-01-30 16:15:29"), ShipmentNumber = 0 },
        new license { LicenseId = 214, PartMasterId = 11, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2007-11-23 19:52:05"), IsActive = false, GracePeriodDays = 30, LicenseTermMonths = 3, ShipDate = DateTime.Parse("2007-07-23 00:00:00"), ShipmentNumber = 75 },
        new license { LicenseId = 285, PartMasterId = 6, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2007-09-23 19:52:31"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 1, ShipDate = DateTime.Parse("2007-08-23 19:46:17"), ShipmentNumber = 0 },
        new license { LicenseId = 647, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2008-09-23 19:52:31"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 12, ShipDate = DateTime.Parse("2007-11-27 14:15:43"), ShipmentNumber = 0 },
        new license { LicenseId = 3323, PartMasterId = 17, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2009-09-23 19:52:31"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 12, ShipDate = DateTime.Parse("2008-10-29 11:18:44"), ShipmentNumber = 0 },
        new license { LicenseId = 23562, PartMasterId = 211, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2012-09-19 08:56:39"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 30, ShipDate = DateTime.Parse("2010-03-19 08:56:39"), ShipmentNumber = 0 },
        new license { LicenseId = 23563, PartMasterId = 39, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2012-09-19 08:57:32"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 30, ShipDate = DateTime.Parse("2010-03-19 08:57:32"), ShipmentNumber = 0 },
        new license { LicenseId = 26517, PartMasterId = 17, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2012-09-23 19:52:31"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2010-04-20 21:04:43"), ShipmentNumber = 0 },
        new license { LicenseId = 388874, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2012-12-23 19:52:31"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 3, ShipDate = DateTime.Parse("2012-12-18 05:47:32"), ShipmentNumber = 0 },
        new license { LicenseId = 391688, PartMasterId = 39, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2015-12-20 11:20:06"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-12-20 11:20:06"), ShipmentNumber = 0 },
        new license { LicenseId = 391689, PartMasterId = 211, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2015-12-20 11:21:16"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 36, ShipDate = DateTime.Parse("2012-12-20 11:21:16"), ShipmentNumber = 0 },
        new license { LicenseId = 414517, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2013-01-23 19:52:31"), IsActive = false, GracePeriodDays = 0, LicenseTermMonths = 1, ShipDate = DateTime.Parse("2013-01-15 22:23:45"), ShipmentNumber = 0 },
        new license { LicenseId = 471016, PartMasterId = 19, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2019-01-23 19:52:31"), IsActive = true, GracePeriodDays = 72, LicenseTermMonths = 72, ShipDate = DateTime.Parse("2013-03-13 18:19:09"), ShipmentNumber = 0 },
        new license { LicenseId = 648249, PartMasterId = 782, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2013-09-24 15:43:26"), IsActive = true, GracePeriodDays = 36, LicenseTermMonths = 0, ShipDate = DateTime.Parse("2013-08-19 15:43:26"), ShipmentNumber = 0 },
        new license { LicenseId = 648250, PartMasterId = 579, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 37, ExpirationDate = DateTime.Parse("2013-09-24 15:44:53"), IsActive = true, GracePeriodDays = 36, LicenseTermMonths = 0, ShipDate = DateTime.Parse("2013-08-19 15:44:53"), ShipmentNumber = 0 },
      };

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var l in PanLicensesBasic) {
					sb.Append(string.Format("INSERT INTO license(LicenseId, PartMasterId, SupportAccountId, SupportCenterSupportAccountId, DeviceId, ExpirationDate, IsActive, GracePeriodDays, LicenseTermMonths, ShipDate, ShipmentNumber) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, '{9}', {10});", l.LicenseId, l.PartMasterId, l.SupportAccountId, l.SupportCenterSupportAccountId.HasValue ? l.SupportCenterSupportAccountId.Value.ToString() : "null", l.DeviceId, l.ExpirationDate.HasValue ? "'" + l.ExpirationDate.Value.ToString(MySqlDateFormat) + "'" : "null", l.IsActive ? 1 : 0, l.GracePeriodDays, l.LicenseTermMonths, l.ShipDate.ToString(MySqlDateFormat), l.ShipmentNumber));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
        #endregion  


        public static readonly license[] AscLicensesBasic = new license[]
      {
        new license { LicenseId = 10001, PartMasterId = 2, SupportAccountId = 245, SupportCenterSupportAccountId = null, DeviceId = 1001, ExpirationDate = DateTime.Parse("2015-03-08 14:52:33"), IsActive = true, GracePeriodDays = 30, LicenseTermMonths = 12, ShipDate = DateTime.Parse("2014-03-02 00:00:00"), ShipmentNumber = 0 },
        new license { LicenseId = 10003, PartMasterId = 165, SupportAccountId = 245, SupportCenterSupportAccountId = 3, DeviceId = 1001, ExpirationDate = DateTime.Parse("2015-10-02 14:20:24"), IsActive = true, GracePeriodDays = 0, LicenseTermMonths = 2, ShipDate = DateTime.Parse("2014-08-02 14:20:14"), ShipmentNumber = 0 },
       
      };

        public static void AscSeed()
        {
            using (var db = new cspEntities())
            {

                var sb = new StringBuilder();
                foreach (var l in AscLicensesBasic)
                {
                    sb.Append(string.Format("INSERT INTO license(LicenseId, PartMasterId, SupportAccountId, SupportCenterSupportAccountId, DeviceId, ExpirationDate, IsActive, GracePeriodDays, LicenseTermMonths, ShipDate, ShipmentNumber) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, '{9}', {10});", l.LicenseId, l.PartMasterId, l.SupportAccountId, l.SupportCenterSupportAccountId.HasValue ? l.SupportCenterSupportAccountId.Value.ToString() : "null", l.DeviceId, l.ExpirationDate.HasValue ? "'" + l.ExpirationDate.Value.ToString(MySqlDateFormat) + "'" : "null", l.IsActive ? 1 : 0, l.GracePeriodDays, l.LicenseTermMonths, l.ShipDate.ToString(MySqlDateFormat), l.ShipmentNumber));
                }

                db.Database.ExecuteSqlCommand(sb.ToString());
            }
        }
	}
}