using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IEmailNotificationLogic
    {
        EmailNotification QueueEmailNotification(string emailsubject, string emailBody, List<string> toList, List<string> ccList, List<string> bccList,
            long createdByUserAccountId, long countToBeSent, string addressing, string featureName, bool isEmailOptOutEnabled = true);
            
        void UpdatePendingEmailNotificationStatus();
    }
}
