using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using FluentNHibernate.Conventions;

namespace Csp.Infrastructure.Data.Persistence
{
    public class AscScoreCardRepository : IAscScoreCardRepository
    {
        public DateTime? GetAscDataLastUploadedOn(long ascId)
        {
            DateTime? lastUploadedOn = null;

            using (var db = new cspEntities())
            {
                var ascData = db.ascdatas.Where(x => x.AscId == ascId).OrderByDescending(x => x.LoadedOn).FirstOrDefault();

                if (ascData != null)
                {
                    lastUploadedOn = ascData.LoadedOn;
                }
            }

            return lastUploadedOn;
        }

        public void InsertAscData(List<AscDataModel> ascData)
        {
            long ascId = 0;
            string processedMonth = string.Empty;

            if (ascData.Any())
            {
                ascId = ascData[0].AscId;
                processedMonth = ascData[0].ProcessedMonth;
            }

            using (var db = new cspEntities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                var dataItems = db.ascdatas.Where(x => x.AscId == ascId && x.ProcessedMonth == processedMonth && x.ProcessedOn == null).ToArray();

                if (dataItems.Any())
                {
                    foreach (var item in dataItems)
                    {
                        db.ascdatas.Remove(item);
                    }

                    db.SaveChanges();
                }

                if (ascData.Any())
                {
                    foreach (var item in ascData)
                    {
                        db.ascdatas.Add(new ascdata()
                        {
                            AscId = item.AscId,
                            SalesforceAccountId = item.SalesforceAccountId,
                            CreatedOn = item.CreatedOn,
                            CaseNumber = item.CaseNumber,
                            CustomerName = item.CustomerName,
                            CustomerEmail = item.CustomerEmail,
                            ClosedOn = item.ClosedOn,
                            LoadedOn = DateTime.Now,
                            ProcessedMonth = item.ProcessedMonth
                        });
                    }

                    db.SaveChanges();
                }
            }
        }


        public bool betaAccess(long supportAccountId)
        {
            using (var db = new cspEntities())
            {
                return db.betaaccesses.Any(b => b.FeatureName.Equals("ASC_SELFSERVICE_DASHBOARD") && b.SupportAccountId == supportAccountId);
            }

        }

        public List<string> GetAscList()
        {
            const short ascSecurityClassification = (short)SecurityClassification.Support;
            using (var db = new cspEntities())
            {
                var ascList = (from at in db.accounttypes
                               join sa in db.supportaccounts on at.AccountTypeId equals sa.AccountTypeId
                               join mem in db.memberships on sa.SupportAccountId equals mem.SupportAccountId
                               join mr in db.membershiproles on mem.MembershipId equals mr.MembershipId

                               where at.Name == "ASC"
                                     && mem.ActivationDate <= DateTime.Now
                                     && (
                                         mem.ExpirationDate == null
                                         || mem.ExpirationDate >= DateTime.Now
                                         )
                                     && mr.role.SecurityClassification == ascSecurityClassification

                               select mem.useraccount.Email).Distinct().ToList();

                return ascList;
            }
        }

        public int InsertAscScorecardReminder(string status)
        {
            int ascScorecardReminderId = 0;
            using (var db = new cspEntities())
            {
                var r = new ascscorecardreminder()
                {
                    Status = status
                };
                db.ascscorecardreminders.Add(r);

                if (db.SaveChanges() > 0)
                {
                    ascScorecardReminderId = Convert.ToInt32(r.ASCScorecardReminderId);
                }

                return ascScorecardReminderId;
            }
        }

        public bool UpdateAscScorecardReminderStatus(int ascScorecardReminderId,string status)
        {
            using (var db = new cspEntities())
            {
                if (db.ascscorecardreminders.Any(a=>a.ASCScorecardReminderId.Equals(ascScorecardReminderId)))
                {
                    var originalRecord = db.ascscorecardreminders.Find(ascScorecardReminderId);
                    originalRecord.Status = status;
                    db.SaveChanges();
                    return true;
                }

                return false;
            }
        }

        public int InsertAscScorecardReminderEmailList(int ascScorecardReminderId, string emailList, string status)
        {
            int ascReminderEmailListId = 0;
            using (var db = new cspEntities())
            {
                var r = new ascreminderemaillist()
                {
                    ASCScorecardReminderId = ascScorecardReminderId,
                    EmailList = emailList,
                    Status = status
                };
                db.ascreminderemaillists.Add(r);

                if (db.SaveChanges() > 0)
                {
                    ascReminderEmailListId = Convert.ToInt32(r.ASCReminderEmailListId);
                }

                return ascReminderEmailListId;
            }
        }

    }
}
