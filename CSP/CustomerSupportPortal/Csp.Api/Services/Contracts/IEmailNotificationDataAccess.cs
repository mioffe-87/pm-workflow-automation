using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IEmailNotificationDataAccess
    {
        bool LogEmailNotificationByNotificationId(LogEmailNotification logEmailNotification);
        List<string> GetPendingEmailNotificationsList();
        void UpdateEmailNotificationStatus(long countProcessed, string awsNotificationId);
        EmailTemplate GetEmailTemplateByTemplateId(int templateId);
    }
}
