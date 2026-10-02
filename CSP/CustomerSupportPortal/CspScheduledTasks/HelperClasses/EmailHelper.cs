using System;
using System.Collections.Generic;
using System.Configuration;
using System.EnterpriseServices;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CspScheduledTasks.HelperClasses
{
    public class EmailHelper
    {
        public static bool SendMail(string emailTo, string subject, string message)
        {
            try
            {
                var instanceName = ConfigurationManager.AppSettings["InstanceName"];

                using (var smtp = new SmtpClient())
                {
                    var msg = new MailMessage
                    {
                        Subject = string.Format("{0} {1}", subject, instanceName),
                        Body = message
                    };

                    //set the message recipients
                    var tos = emailTo.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var to in tos)
                    {
                        msg.To.Add(to);
                    }

                    smtp.Send(msg);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
