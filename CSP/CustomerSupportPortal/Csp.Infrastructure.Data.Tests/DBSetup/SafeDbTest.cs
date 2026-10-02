using System.IO;
using System.Linq;

namespace Csp.Infrastructure.Data.Tests.DBSetup {
	public class SafeDbTest {

		protected SafeDbTest() {
			VerifyTestDbConnection();
		}

		private void VerifyTestDbConnection() {

			using (var db = new DBs.CSP.cspEntities()) {
				var dbName = db.Database.Connection.Database;
				//if (dbName != "dbtests_csp") {
				//	throw new InvalidDataException("Wrong DB Danger Danger!!!!");
				//}
				var dataSource = db.Database.Connection.DataSource;
				if (dataSource.Contains("sjccsqld") || dataSource.Contains("sjccsqlt") || dataSource.Contains("sjccsqlp")) {
					throw new InvalidDataException("Wrong DB Danger Danger!!!!");
				}
			}
		}

		protected void ReSeedData() {

			ClearData();

			SeedData();
		}

		private const string TruncateStatement = "TRUNCATE TABLE csp.{0};";

		/// <summary>
		/// SELECT CONCAT('db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "', TABLE_NAME, '"));') AS TruncateStatement FROM information_schema.TABLES T WHERE T.TABLE_SCHEMA = 'dbtests_csp' AND T.TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
		/// </summary>
		protected void ClearData() {
			using (var db = new DBs.CSP.cspEntities()) {
				db.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=0;");
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "accounttype"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "accounttypeassignableroles"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "accounttypedefaultroles"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "ascinformationreview"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "bundle"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "contentupdates"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "country"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "csphelp"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "device"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "devicecategory"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "devicedefaultroles"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "devicesets"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "devicesetsdevices"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "devicetransfers"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "devicetransferstates"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "domain"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "duplicatesap"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "duplicatesap_total"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "evalaccountcreationlogs"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "evalextensionhistory"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "getsfdcforserialnumbers"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "groupdefaultroles"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "groupdevices"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "grouplicenses"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "jivetemp"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "license"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logaccounttransfer"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logaccounttransferdetail"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logdevice"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logemailnotification"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logemailnotificationemaillist"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "loglicense"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logmembership"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logmembershiproles"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "lognewuser"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logregistration"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logsapsupportaccountmapping"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logsupportaccount"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "logtransaction"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "loguseraccount"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "lookups"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "membership"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "membershiproles"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "models"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "partmaster"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "partsubtype"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "parttype"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "privilege"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "publishtargets"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "region"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "resourcelinks"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "role"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "rolecategories"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "roleprivileges"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "rolesecurityclassifications"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "salesorder"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "salesorderdata"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "salesorderdata_6thdec"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "salesorderdata_delta"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "salesorderitem"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "salesordertemp"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "sapcustomer"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "sapsupportaccountmapping"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "softwareupdates"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "supportaccount"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "supportaccountdefaultroles"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "supportaccountdomains"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "supportaccountgroup"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "useraccount"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "userselectedmembership"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "virusupdates"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "vmevals"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "vmstubsns"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "wildfireapikey"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "wildfiredevice"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "wildfiredevicelog"));
				db.Database.ExecuteSqlCommand(string.Format(TruncateStatement, "wildfireuseraccount"));
				db.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=1;");

			}
		}

		private void SeedData() {

			Seed.CountryData.Seed();

			Seed.RoleCategoryData.Seed();

			Seed.RoleSecurityClassificationsData.Seed();

			Seed.RoleData.Seed();

			Seed.AccountTypeData.Seed();

			Seed.SupportAccountData.Seed();

			Seed.UserAccountData.Seed();

			Seed.PartTypeData.Seed();

			Seed.PartSubTypeData.Seed();

			Seed.PartMasterData.Seed();

			Seed.DeviceData.VanillaSeed();

			Seed.LicenseData.Seed();
		}
	}
}
