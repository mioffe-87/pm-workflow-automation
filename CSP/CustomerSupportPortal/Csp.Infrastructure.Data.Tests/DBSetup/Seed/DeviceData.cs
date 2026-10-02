using System;
using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class DeviceData {

		private const string MySqlDateFormat = "yyyy-MM-dd HH:mm:ss";

        //SELECT CONCAT('new device{ DeviceId = ', d.DeviceId, ', PartMasterId = ', d.PartMasterId, ', SerialNumber = "', d.SerialNumber, '", SupportCenterSupportAccountId = null, SupportAccountId = ', d.SupportAccountId, ', IsActive = ', IF(d.IsActive = 1, 'true', 'false'), ', ShipDate = DateTime.Parse("', d.ShipDate, '") }, ')
        #region Basic Seed
        public static readonly device[] PanDeviceBasic = new device[]
      {
        new device{ DeviceId = 2, PartMasterId = 10, SerialNumber = "PA04070003", SupportCenterSupportAccountId = null, SupportAccountId = 245, IsActive = true, ShipDate = DateTime.Parse("2007-01-15 00:00:00") }, 
        new device{ DeviceId = 11, PartMasterId = 10, SerialNumber = "0001A100105", SupportCenterSupportAccountId = null, SupportAccountId = 245, IsActive = true, ShipDate = DateTime.Parse("2007-03-20 00:00:00") }, 
        new device{ DeviceId = 18, PartMasterId = 10, SerialNumber = "0001A100110", SupportCenterSupportAccountId = null, SupportAccountId = 245, IsActive = true, ShipDate = DateTime.Parse("2007-04-03 00:00:00") }, 
        new device{ DeviceId = 25, PartMasterId = 10, SerialNumber = "0001A100109", SupportCenterSupportAccountId = null, SupportAccountId = 245, IsActive = true, ShipDate = DateTime.Parse("2007-03-09 00:00:00") }, 
        new device{ DeviceId = 37, PartMasterId = 10, SerialNumber = "0001A100236", SupportCenterSupportAccountId = null, SupportAccountId = 245, IsActive = true, ShipDate = DateTime.Parse("2007-07-23 00:00:00") }, 
      };

		public static void VanillaSeed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var d in PanDeviceBasic) {
					sb.Append(string.Format("INSERT INTO device(DeviceId, PartMasterId, SerialNumber, SupportAccountId, IsActive, ShipDate) VALUES({0}, {1}, '{2}', {3}, {4}, '{5}');", d.DeviceId, d.PartMasterId, d.SerialNumber, d.SupportAccountId, d.IsActive ? 1 : 0, d.ShipDate.ToString(MySqlDateFormat)));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
            #endregion


        #region ASC Seed
        public static readonly device[] AscDevices = new device[]
      {
        new device{ DeviceId = 1001, PartMasterId = 10, SerialNumber = "ASC00001", SupportCenterSupportAccountId = 3, SupportAccountId = 92, IsActive = true, ShipDate = DateTime.Parse("2007-01-15 00:00:00") }, 
     
      };

        public static void AscDevicesSeed()
        {
            using (var db = new cspEntities())
            {

                var sb = new StringBuilder();
                foreach (var ad in AscDevices)
                {
                    sb.Append(string.Format("INSERT INTO device(DeviceId, PartMasterId, SerialNumber, SupportAccountId, IsActive, ShipDate, SupportCenterSupportAccountId) VALUES({0}, {1}, '{2}', {3}, {4}, '{5}', {6});", 
                        ad.DeviceId, ad.PartMasterId, ad.SerialNumber, ad.SupportAccountId, ad.IsActive ? 1 : 0, ad.ShipDate.ToString(MySqlDateFormat), ad.SupportCenterSupportAccountId));
                }

                db.Database.ExecuteSqlCommand(sb.ToString());
            }
        }
        #endregion
    }
}
