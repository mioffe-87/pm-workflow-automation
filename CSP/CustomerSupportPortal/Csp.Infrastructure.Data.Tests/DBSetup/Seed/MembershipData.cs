using System;
using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	class MembershipData : SeedHelper {

		/* SELECT CONCAT('new membership{MembershipId = ', m.MembershipId, ', UserAccountId = ', m.UserAccountId, ', ActivationDate = DateTime.Parse("', m.ActivationDate, 
				'"), ExpirationDate = ', CASE WHEN ISNULL(m.ExpirationDate) THEN 'null' ELSE CONCAT('DateTime.Parse("', m.ExpirationDate, '")') END, 
				', SupportAccountId = ', CASE WHEN ISNULL(m.SupportAccountId) THEN 'null' ELSE m.SupportAccountId END, 
				', GroupId = ', CASE WHEN ISNULL(m.GroupId) THEN 'null' ELSE m.GroupId END, 
				', DeviceId = ', CASE WHEN ISNULL(m.DeviceId) THEN 'null' ELSE m.DeviceId END, 
				', JiveId = "', m.JiveId, '", SalesforceContactId = "', m.SalesforceContactId, '", SalesforceLogInId = "', m.SalesforceLogInId, '"},') FROM membership m
		 */

		private static readonly membership[] StandardMemberships = new membership[]
			{
				new membership{MembershipId = 1, UserAccountId = 40893, ActivationDate = DateTime.Parse("2013-01-30 11:08:31"), ExpirationDate = null, SupportAccountId = 92, GroupId = null, DeviceId = null, JiveId = "", SalesforceContactId = "", SalesforceLogInId = ""},
			};

		private static readonly membershiprole[]  StandardMembershipRoles= new membershiprole[]
			{
				new membershiprole{MembershipId = 1, RoleId = 1},
			};

		public static void SeedStandardMembership() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var sa in StandardMemberships) {
					sb.AppendLine(string.Format("INSERT INTO membership (MembershipId, UserAccountId, ActivationDate, ExpirationDate, SupportAccountId, GroupId, DeviceId, JiveId, SalesforceContactId, SalesforceLogInId, CreatedOn, CreatedByUserAccountId) " +
						"VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, '{7}', '{8}', '{9}', {10}, {11});", sa.MembershipId, sa.UserAccountId, NullableDate(sa.ActivationDate), NullableDate(sa.ExpirationDate), NullableLong(sa.SupportAccountId), NullableLong(sa.GroupId), NullableLong(sa.DeviceId), sa.JiveId, sa.SalesforceContactId, sa.SalesforceLogInId,
						NullableDate(DateTime.Now), 1));
				}

				foreach (var sa in StandardMembershipRoles) {
					sb.AppendLine(string.Format("INSERT INTO membershiproles (MembershipId, RoleId) VALUES({0}, {1});", sa.MembershipId, sa.RoleId));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}








        private static readonly supportaccountgroup[] SuppoartAccountGroupMemberships = new supportaccountgroup[]
			{
				new supportaccountgroup{ SupportAccountId = 92, GroupId = 1, Name = "TEST GROUP"},
			};

        private static readonly membership[] GroupMemberships = new membership[]
			{
				new membership{MembershipId = 1, UserAccountId = 40893, ActivationDate = DateTime.Parse("2013-01-30 11:08:31"), ExpirationDate = null, SupportAccountId = null, GroupId = 1, DeviceId = null, JiveId = "", SalesforceContactId = "", SalesforceLogInId = ""},
			};

        private static readonly membershiprole[] GroupMembershipRoles = new membershiprole[]
			{
				new membershiprole{MembershipId = 1, RoleId = 5},
			};


        public static void SeedGroupMembership()
        {
            using (var db = new cspEntities())
            {

                var sb = new StringBuilder();


                //loop through supportaccountgroups and insert them

                foreach (var sag in SuppoartAccountGroupMemberships)
                {
                    sb.AppendLine(string.Format("INSERT INTO supportaccountgroup (GroupId, SupportAccountId, Name) " +
                        "VALUES({0}, {1}, '{2}');", sag.GroupId, sag.SupportAccountId, sag.Name));
                }


                foreach (var grp in GroupMemberships)
                {
                    sb.AppendLine(string.Format("INSERT INTO membership (MembershipId, UserAccountId, ActivationDate, ExpirationDate, SupportAccountId, GroupId, DeviceId, JiveId, SalesforceContactId, SalesforceLogInId, CreatedOn, CreatedByUserAccountId) " +
                        "VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, '{7}', '{8}', '{9}', {10}, {11});", grp.MembershipId, grp.UserAccountId, NullableDate(grp.ActivationDate), NullableDate(grp.ExpirationDate), NullableLong(grp.SupportAccountId), NullableLong(grp.GroupId), NullableLong(grp.DeviceId), grp.JiveId, grp.SalesforceContactId, grp.SalesforceLogInId,
                        NullableDate(DateTime.Now), 1));
                }

                foreach (var grpRole in GroupMembershipRoles)
                {
                    sb.AppendLine(string.Format("INSERT INTO membershiproles (MembershipId, RoleId) VALUES({0}, {1});", grpRole.MembershipId, grpRole.RoleId));
                }

                db.Database.ExecuteSqlCommand(sb.ToString());
            }
        }

        
        
       private static readonly membership[] AscMemberships = new membership[]
			{
				new membership{MembershipId = 1, UserAccountId = 40893, ActivationDate = DateTime.Parse("2013-01-30 11:08:31"), ExpirationDate = null, SupportAccountId = 3, GroupId = null, DeviceId = null, JiveId = "", SalesforceContactId = "", SalesforceLogInId = ""},
			};

        private static readonly membershiprole[] AscMembershipRoles = new membershiprole[]
			{
				new membershiprole{MembershipId = 1, RoleId = 3},
			};
        
        public static void SeedAscMembership()
        {
            using (var db = new cspEntities())
            {

                var sb = new StringBuilder();
                
                foreach (var sa in AscMemberships)
                {
                    sb.AppendLine(string.Format("INSERT INTO membership (MembershipId, UserAccountId, ActivationDate, ExpirationDate, SupportAccountId, GroupId, DeviceId, JiveId, SalesforceContactId, SalesforceLogInId, CreatedOn, CreatedByUserAccountId) " +
                        "VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, '{7}', '{8}', '{9}', {10}, {11});", sa.MembershipId, sa.UserAccountId, NullableDate(sa.ActivationDate), NullableDate(sa.ExpirationDate), NullableLong(sa.SupportAccountId), NullableLong(sa.GroupId), NullableLong(sa.DeviceId), sa.JiveId, sa.SalesforceContactId, sa.SalesforceLogInId,
                        NullableDate(DateTime.Now), 1));
                }

                foreach (var ascRole in AscMembershipRoles)
                {
                    sb.AppendLine(string.Format("INSERT INTO membershiproles (MembershipId, RoleId) VALUES({0}, {1});", ascRole.MembershipId, ascRole.RoleId));
                }

                db.Database.ExecuteSqlCommand(sb.ToString());
            }
        }



	}
}