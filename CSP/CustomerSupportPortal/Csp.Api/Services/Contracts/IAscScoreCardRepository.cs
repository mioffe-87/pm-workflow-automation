using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IAscScoreCardRepository
    {
        DateTime? GetAscDataLastUploadedOn(long ascId);
        void InsertAscData(List<AscDataModel> ascData);
        bool betaAccess(long supportAccountId);

        List<string> GetAscList();
        int InsertAscScorecardReminder(string status);
        int InsertAscScorecardReminderEmailList(int ascScorecardReminderId, string emailList, string status);
        bool UpdateAscScorecardReminderStatus(int ascScorecardReminderId, string status);
    }
}
