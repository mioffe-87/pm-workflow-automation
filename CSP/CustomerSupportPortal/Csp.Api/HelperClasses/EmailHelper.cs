using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.DataAccess;
using Csp.Api.Logic.EvalWebService;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using SendGridMail;
using SendGridMail.Transport;
using System.Threading.Tasks;

namespace Csp.Api.HelperClasses
{

    public class EmailHelper : IEmailHelper
    {

        /// <summary>
        /// This method sends the email of the results of a bulk registration to the email of the user that initiated it. An email is sent as the processing could take longer
        /// than an acceptable wait time. For this reason the processing happens in a separate thread and calls this method to send the email when completed. 
        /// </summary>
        /// <param name="email">The email address of the user that performed the bulk registration.</param>
        /// <param name="deviceErrorLog">A dictionary of error messages that holds a list of each serial numbers the message applies to.</param>
        /// <param name="licenseErrorLog">A dictionary of error messages that holds a list of each authorization code numbers the message applies to.</param>
        /// <param name="criticalErrorLog">A list of messages for critical errors that occured during the bulk registration process.</param>
        public static void SendBulkRegistrationEmail(string email, Dictionary<string, List<string>> deviceErrorLog, Dictionary<string, List<string>> licenseErrorLog, List<string> criticalErrorLog)
        {
            SendBulkRegistrationEmail(string.Empty, email, deviceErrorLog, licenseErrorLog, criticalErrorLog);
        }

        public static void SendBulkRegistrationEmail(string bulkRegistrationHistoryUrl, string email, Dictionary<string, List<string>> deviceErrorLog,
            Dictionary<string, List<string>> licenseErrorLog, List<string> criticalErrorLog)
        {

            //This body of the email message which uses a string builder for optimized creation of the body
            var body = new StringBuilder();

            //The subject of the email
            var subject = "Bulk Registration Results";

            var urlContent = string.Empty;

            if (!string.IsNullOrEmpty(bulkRegistrationHistoryUrl))
            {
                urlContent = string.Format(@"
                            <BR/><BR/>
                            Please click on the following link or copy-paste it in your browser to view your devices and licenses:
                            <BR/><BR/>
                            <a href=""{0}"">{0}</a>
                            <BR/><BR/>", bulkRegistrationHistoryUrl);
            }

            //If there are no errors then simply notify the user that all devices and authorization codes have been sucessfully registered. 
            if (deviceErrorLog.Keys.Count == 0 && licenseErrorLog.Keys.Count == 0 && criticalErrorLog.Count == 0)
            {

                body.Append("Thank you for registering your devices and authorization codes. All devices and authorization codes have been successfully registered.");

                if (!string.IsNullOrEmpty(urlContent))
                {
                    body.Append(urlContent);
                }
            }
            //Otherwise if there are errors then start notifying the user of the errors that occured. 
            else
            {

                //Notification for registration of all devices and authorization codes with exceptions listed below
                body.Append("Thank you for registering your devices and authorization codes. There were problems during the registration process.");


                if (!string.IsNullOrEmpty(urlContent))
                {
                    body.Append(urlContent);
                }
                else
                {
                    body.Append("<BR />");
                }

                //List out all of the critical errors
                if (criticalErrorLog.Count > 0)
                {
                    body.Append("<BR />");
                    body.Append("----------------------------------------------------------");
                    body.Append("<BR />");
                    foreach (var criticalError in criticalErrorLog)
                    {
                        body.Append(criticalError);
                        body.Append("<BR />");
                    }
                }

                //If there are any messages in the device error logs then add to the email message
                if (deviceErrorLog.Keys.Count > 0)
                {
                    body.Append("<BR />");
                    body.Append("----------------------------------------------------------");
                    body.Append("<BR />");

                    //loop through the items in the dictionary retrieving the key/value pair
                    foreach (var deviceErrorKvp in deviceErrorLog)
                    {

                        //Add the error message.
                        body.Append(deviceErrorKvp.Key);
                        body.Append("<BR />");
                        body.Append("<BR />");

                        //Loop through the serial numbers under the message
                        foreach (var deviceserialNumber in deviceErrorKvp.Value)
                        {

                            //Add each serial number under the error message to inform the user of the devices that failed due to the error
                            body.Append(deviceserialNumber);
                            body.Append("<BR />");
                        }
                    }
                }

                //If there are any messages in the license error logs then add to the email message
                if (licenseErrorLog.Keys.Count > 0)
                {
                    body.Append("<BR />");
                    body.Append("----------------------------------------------------------");
                    body.Append("<BR />");

                    //loop through the items in the dictionary retrieving the key/value pair
                    foreach (var licenseErrorKvp in licenseErrorLog)
                    {

                        //Add the error message.
                        body.Append(licenseErrorKvp.Key);
                        body.Append("<BR />");
                        body.Append("<BR />");

                        //Loop through the serial numbers and auth codes under the message
                        foreach (var serialAndAuthCode in licenseErrorKvp.Value)
                        {

                            /*Add each serial number and authcode (already formated when added to the error log) under the error message to inform the user 
                             * of the authcodes that failed to activate, and the device the authcode failed on. 
                             */
                            body.Append(serialAndAuthCode);
                            body.Append("<BR />");
                        }
                    }
                }

            }

            //Send the email
            SendMail(email, subject, body.ToString());

        }

        private const int MaxSubjectLength = 100;

        public static void SendExceptionEmail(Exception ex)
        {
            var body = new StringBuilder();

            body.Append("Internal Exception Occurred");
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Msg:");
            body.Append("<BR />");
            body.Append(ex.Message);
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Time:         " + DateTime.Now.ToString());
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("User:      " + HttpContext.Current.User.Identity.Name);
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Machine:      " + Environment.MachineName);
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Url:      " + HttpContext.Current.Request.RawUrl);
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Source:      ");
            body.Append(ex.Source);
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Target Site:      ");
            body.Append("<BR />");
            body.Append(ex.TargetSite);
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Trace:");
            body.Append("<BR />");
            body.Append(ex.StackTrace);
            body.Append("<BR />");
            body.Append("----------------------------------------------------------");
            body.Append("<BR />");
            body.Append("Data:");
            body.Append("<BR />");
            body.Append(ex.Data);
            body.Append("<BR />");


            var configValues = new ConfigValues();
            var emailTo = configValues.GetAppSetting(AppSettings.ErrorEmailTo).Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var subject = configValues.GetAppSetting(AppSettings.AppName) + " - Exception: ";

            if (subject.Length > MaxSubjectLength)
                subject = subject.Substring(0, MaxSubjectLength);

            // Use ENS instead of Smtp
            //SendSmtpMail(emailTo, subject, null, body.ToString());
            // SendMail(string.Join(",", emailTo), subject, body.ToString(), false, "ExceptionEmail");
        }

        public static void SendUserActivationEmail(string email, string name, string activationCode, bool isMembershipPending = false)
        {
            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();

            var configValues = new ConfigValues();
            var userActivationUrl = configValues.GetAppSetting(AppSettings.UserActivationUrl);
            var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

            userActivationUrl = string.Format(userActivationUrl, activationCode);

            var membershipPendingText = string.Empty;
            var extraMembershipPendingText = string.Empty;
            if (isMembershipPending)
            {
                membershipPendingText = "In addition to activation, this account requires Super User approval.  Your Super User has been notified. You will receive another notification after your Super User approves or denies your access.<BR/><BR/>";
                extraMembershipPendingText = " and super user approval";
            }

            var templateId = Convert.ToInt32(TemplateId.SetPassword);
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(templateId);

            if (!string.IsNullOrEmpty(emailTemplate.Subject))
            {
                emailTemplate.Subject = string.Format(emailTemplate.Subject, name);
            }

            if (!string.IsNullOrEmpty(emailTemplate.Content))
            {
                emailTemplate.Content = string.Format(emailTemplate.Content, name, userActivationUrl, email, membershipPendingText, extraMembershipPendingText);
            }

          //  var subject = string.Format("Account Activation Details for {0} at Palo Alto Networks Support Site", name);
          //  var content = string.Format(@"
          //                  <BR/>
          //                  Hello {0},
          //                  <BR/><BR/>
          //                  Thank you for registering at Palo Alto Networks. Your account is created and must be activated before you can use it.
          //                  <BR/><BR/>
          //                  To activate the account click on the following link or copy-paste it in your browser:
          //                  <BR/><BR/>
          //                  <a href=""{1}"">{1}</a>
          //                  <BR/><BR/>{4}
          //                  After activation{5} you may login to <a href=""{2}"">{2}</a> using the following e-mail and password you registered with:
          //                  <BR/><BR/>
          //                  E-mail - {3}
          //                  <BR/>
          //                  Password - [Your Password]
          //                  <BR/><BR/>

          //                  <u style='font-weight: bold;'>Registering as a Customer of Palo Alto Networks unlocks:</u>
          //                  <BR/>
          //                  <BR/>

          //                  <a style='color: black; font-weight: bold;' href='https://support.paloaltonetworks.com/'>Customer Support Portal</a>
          //                  - Use the Customer Support Portal to: Create & manage support cases.  Register your Palo Alto Network assets.  Create Day 1 Configurations for your NGFW devices. Run a Best-Practice Assessment, and more. You also automatically get a Live Community account: Get knowledge & answers to questions, and full access to the Live Community.

          //                  <BR/>
          //                  <BR/>
          //                  <a style='color: black; font-weight: bold;' href='https://beacon.exceedlms.com/'>Beacon</a>
          //                  - Beacon is an interactive, engaging one-stop site, that connects all Palo Alto Networks resources you need in one location, reducing the time you’ll spend onboarding, researching and getting to know our tools, technology and services.

          //                  <BR/>
          //                  <BR/>
          //                  <a style='color: black; font-weight: bold;' href='https://live.paloaltonetworks.com/'>LIVEcommunity</a>
          //                  - The Live Community provides access to thousands of knowledge base articles to help you troubleshoot and solve your technical challenges. In addition, the Discussions area connects you with over 35,000 Palo Alto Networks users and experts to ask questions, get advice, and learn about how you can get more from your Palo Alto Networks technology.
          //                  <BR/>
          //                  <BR/>
							
          //                  Thank you,<br/><br/>
						    //Palo Alto Networks", name, userActivationUrl, applicationUrl, email, membershipPendingText, extraMembershipPendingText);

            SendMail(email, emailTemplate.Subject, emailTemplate.Content, false, "User Activation");
        }

        public static void SendDpmFeatureActivationEmail(string email, string adminEmails, string serialNumber)
        {
            var subject = string.Format("Decryption Port Mirror feature enabled on {0}", serialNumber);
            var content = string.Format(@"
                            Dear Account Administrator:
														<BR/><BR/>
														An administrator with email {0} has accessed the Palo Alto Networks support portal and enabled the ability to view and store SSL decrypted sessions 
														(e.g., Hypertext Transfer Protocol Secure or HTTPS) on a device with serial number {1}.  A country or jurisdiction that governs the SSL sessions may have regulations or laws prohibiting 
														this conduct.  
														<BR/><BR/>
														It is your responsibility to ensure that your Company’s decryption of the SSL sessions and subsequent use of the decrypted SSL sessions are permissible under all applicable regulations or laws.  
														Palo Alto Networks is not responsible for any failure by your Company to comply with the applicable regulation or law.
														<BR/><BR/>
														Thank you,
														Palo Alto Networks", email, serialNumber);

            SendMail(adminEmails, subject, content, false, "DPM Enabled");

            content += "<br/><br/>SENT TO: " + adminEmails;

            var configValues = new ConfigValues();
            SendMail(configValues.GetAppSetting(AppSettings.CspSupportEmail), subject, content, false, "DPM Activation");
        }

        public static void SendDpbFeatureActivationEmail(string email, string adminEmails, string serialNumber)
        {
            var subject = string.Format("SSL Port Decryption Broker feature enabled on {0}", serialNumber);
            var content = string.Format(@"
                            Dear Account Administrator:
														<BR/><BR/>
														An administrator with email {0} has accessed the Palo Alto Networks support portal and enabled the ability to decrypt and forward decrypted sessions 
														(e.g., Hypertext Transfer Protocol Secure or HTTPS) on a device with serial number {1}.  A country or jurisdiction that governs the SSL sessions may have regulations or laws prohibiting 
														this conduct.  
														<BR/><BR/>
														It is your responsibility to ensure that your Company’s decryption of the SSL sessions and subsequent use of the decrypted SSL sessions are permissible under all applicable regulations or laws.  
														Palo Alto Networks is not responsible for any failure by your Company to comply with the applicable regulation or law.
														<BR/><BR/>
														Thank you,
														Palo Alto Networks", email, serialNumber);

            SendMail(adminEmails, subject, content, false, "DPB Activation");

            content += "<br/><br/>SENT TO: " + adminEmails;

            var configValues = new ConfigValues();
            SendMail(configValues.GetAppSetting(AppSettings.CspSupportEmail), subject, content, false, "Email to CSR");
        }

        public static bool SendTrialActivationEmail(string email, string name, string type, IEnumerable<ActivationLinkModel> links)
        {
            var subject = type + " subscription trial";
            string content = string.Empty;
            var trial = new StringBuilder();
            var trials = new StringBuilder();
            string firstLinkTemplate = @"<tr style='margin:5px;margin-bottom:15px'><td style='width:45px;'><span width='42' height='42' /></td> <td><span style='font-size: 25px;'>Your requested trial:</span></td> <td></td></tr>";
            string templateDataLakeNote = @"<tr><td> &nbsp;</td><td> Note: A valid Cortex Data Lake instance is required before activating this product.</td><td> &nbsp;</td></tr>";
            string template = @"<tr><td style='width:37px;'><img src='https://d3enxei87delvn.cloudfront.net/{3}' width='37' height='20' /></td>
                <td><b style='font-size: 25px;'>{0}</b></td>

                <td style='text-align:right;'><a href='{1}' style='text-decoration: none;' target='_blank'><span style='background-color: #fa582d;border-color: #fa582d;color: #fff;padding:17px 25px;border-radius:5px;cursor:pointer;'>Activate</span></a></td>
            </tr>
            <tr>
                <td>&nbsp;</td><td><b>Quantity: 1 Firewall</b></td><td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td><td><b>Expires: {2}</b></td><td>&nbsp;</td>
            </tr>
            ";

            if (!string.IsNullOrEmpty(type) && type.Contains("Data Lake"))
            {
                firstLinkTemplate += template + templateDataLakeNote;
            }
            else
            {
                firstLinkTemplate += template;
            }


            string sLogo = "screenshot-1.png";
            if (type.Equals(("SaaS Security Inline")))
                sLogo = "PAN-SaaS-security-blacksm_.png";

            //If it's the 1st link and there are no links requested before.
            if (links != null)
            {
                var activationLinkModels = links.ToList();
                var firstLink = activationLinkModels.First();
                trial.AppendFormat(firstLinkTemplate, type, firstLink.ActivationLink,
                    firstLink.ActivationLinkExpiration.HasValue
                        ? firstLink.ActivationLinkExpiration.Value.ToString("d")
                        : string.Empty, sLogo);

                //Skip the 1st element above(firstLink)
                links = activationLinkModels.Skip(1);

                //If we have more than one link then add the header for previously requested links
                var linkModels = links.ToList();
                if (linkModels.Any())
                {
                    trials.Append(@"<tr style='margin:5px;'><td style='width:45px;'><span width='42' height='42' /></td>
                          <td><span style='font-size: 25px;'>Your company's pending trial waiting on activation:</span></td><td></td></tr>");

                    foreach (var link in linkModels)
                    {
                        if (!string.IsNullOrEmpty(type) && type.Contains("IoT security"))
                        {
                            type = "IoT security";
                        }
                        trials.AppendFormat(template, type, link.ActivationLink,
                            link.ActivationLinkExpiration.HasValue
                                ? link.ActivationLinkExpiration.Value.ToString("d")
                                : string.Empty, sLogo);
                    }
                }

                trials.Insert(0, trial);
                content = string.Format(@"<div style='font-size: 10pt;font-family:arial,helvetica,sans-serif;color:#000000;margin:10px;'>
                <p style='text-align:center;'><img src='https://d3enxei87delvn.cloudfront.net/screenshot-2.png' width='170' height='31' /></p>
                <p><span style='color:#333333;width: 100%;display: block;border-top: solid 1px rgba(128, 128, 128, 0.25);'>&nbsp;</span></p>
                <p>Dear {0},<br></p>
                <p>Thank you for choosing to evaluate Palo Alto Networks software product(s) listed below:</p>
                <table style='width: 100%;'>{1}</table>
                <br>
                <p>Thank you for your interest in evaluating our products.</p>
                <p>If you have any questions about these products, please contact your account team.</p>
                <p>For support please contact Palo Alto Networks Technical Support at:</p>
                <p><b>+1 866-808-9087</b> (within the United States)</p>
                <p><b>+1 408-738-7799</b> (International)</p>
                <p>A complete list of country specific phone numbers can be found <a href='https://www.paloaltonetworks.com/company/contact-sales.html' style='text-decoration: none;' target='_blank'>here.</a></p>
                </div>
                ", name, trials);
            }
            // Use ENS instead of Smtp
            //return SendSmtpMail(new string[] { email }, subject, null, content);
            return SendMail(email, subject, content, false, "SendTrialActivationEmail");
        }

        public static void SendSuperUserRequestForAccess(long supportAccountId, string email, string firstName,
                                                                                                         string lastName)
        {

            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();

            var userEmails =
                                userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId)
                                        .Select(u => u.Email)
                                        .Where(e => !string.IsNullOrEmpty(e.Trim()))
                                        .ToList();

            if (!userEmails.Any())
            {
                return;
            }

            var adminEmails = string.Join(",", userEmails);

            var companyName = supportAccountLogic.GetSupportAccountNameById(supportAccountId);

            var subject = "New User Requesting Access";

            var content = string.Format("Hello,<BR/>" +
                                                                    "<BR/>" +
                                                                    "{0} {1} ({2}) has requested access to the {3} ({4}) account.<br/>" +
                                                                    "<BR/>" +
                                                                    "To provide {0} {1} ({2}) access to the account:<BR/>" +
                                                                    "Login to the Customer Support Portal<BR/>" +
                                                                    "Create a new user.  Refer to <a href='https://live.paloaltonetworks.com/docs/DOC-6279'>How to Create a New User</a><BR/>" +
                                                                    "<BR/>" +
                                                                    "Thank you,<BR/>" +
                                                                    "Palo Alto Networks Support", firstName, lastName, email, companyName, supportAccountId);

            SendMail(adminEmails, subject, content);
        }

        public static void SendUserRegistrationEmailToSuperUsers(string firstName, string lastName, long supportAccountId, long userAccountId,
                                                                long membershipId = 0, bool createdByGroupUser = false)
        {
            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
            var userAccountDataAccess = new UserAccountDataAccess();

            var newUserCreationDetails = userAccountDataAccess.GetNewUserCreationDetails(userAccountId, supportAccountId, membershipId);

            var userEmails = userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId)
                                                .Select(u => u.Email)
                                                .Where(email => !string.IsNullOrEmpty(email.Trim()))
                                                .ToList();
            if (!userEmails.Any())
            {
                return;
            }
            var adminEmails = string.Join(",", userEmails);
            var fullName = string.Format("{0} {1}", firstName, lastName);
            var companyName = supportAccountLogic.GetSupportAccountNameById(supportAccountId);

            const string subject = "New User Added - Super User Notification";
            var content = string.Empty;

            if (createdByGroupUser || membershipId == 0)
            {
                content = string.Format(@"<BR/>Hello<BR/><BR/>
                            A new member {0} has been created by {1} on {2}.
                            <BR/><BR/>
                            Thank you, Palo Alto Networks", fullName, newUserCreationDetails.CreatedByUserName, newUserCreationDetails.CreatedOn.ToShortDateString());
            }
            else
            {
                string newUserDetails = newUserCreationDetails == null ? string.Empty : string.Format(" by {0} on {1} and has been assigned the role(s) of {2}{3}",
                    newUserCreationDetails.CreatedByUserName, newUserCreationDetails.CreatedOn.ToShortDateString(), newUserCreationDetails.RolesAssigned,
                    string.IsNullOrEmpty(newUserCreationDetails.GroupName) ? string.Empty :
                        string.Format(". The new member is added to the Group - {0}", newUserCreationDetails.GroupName));

                content = string.Format(@"
                            <BR/>
                            Hello,
                            <BR/><BR/>
                            This email is to inform you as a SuperUser that a new member, {0} has been added to the account {1}{2}.
                            <BR/><BR/>
                            To verify the members of your account and assign or change roles, please sign into the <a href='https://support.paloaltonetworks.com'>Palo Alto Networks Customer Support Portal</a> 
                            and click on the <a href='https://support.paloaltonetworks.com/SupportAccount/AccountUsers/{3}'>Members</a> tab.
                            <BR/><BR/>
							Thank you,
							Palo Alto Networks", fullName, companyName, newUserDetails, supportAccountId);
            }

            SendMail(adminEmails, subject, content, false, "New User Notification");
        }

        public static void SendUserRegistrationEmail(UserAccountRegistrationEmailModel model, long supportAccountId)
        {

            var configValues = new ConfigValues();
            var emailTo = configValues.GetAppSetting(AppSettings.CspSupportEmail);
            const string subject = "New User Added";
            var content = new StringBuilder();

            if (!String.IsNullOrEmpty(emailTo))
            {
                var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
                var companyName = supportAccountLogic.GetSupportAccountNameById(supportAccountId);

                content.AppendLine(string.Format((string)"FirstName = {0}<br/>", (object)model.UserAccount.FirstName));
                content.AppendLine(string.Format((string)"LastName = {0}<br/>", (object)model.UserAccount.LastName));
                content.AppendLine(string.Format((string)"Title = {0}<br/>", (object)model.UserAccount.Title));
                content.AppendLine(string.Format((string)"Address1 = {0}<br/>", (object)model.UserAccount.Address.Address1));
                content.AppendLine(string.Format((string)"Address2 = {0}<br/>", (object)model.UserAccount.Address.Address2));
                content.AppendLine(string.Format((string)"City = {0}<br/>", (object)model.UserAccount.Address.City));
                content.AppendLine(string.Format((string)"State = {0}<br/>", (object)model.UserAccount.Address.Region));
                content.AppendLine(string.Format((string)"Zip = {0}<br/>", (object)model.UserAccount.Address.PostalCode));
                content.AppendLine(string.Format((string)"Country = {0}<br/>", (object)model.UserAccount.Address.CountryId));
                content.AppendLine(string.Format((string)"Phone = {0}<br/>", (object)model.UserAccount.PhoneNumber));
                content.AppendLine(string.Format((string)"Loginname = {0}<br/>", (object)model.UserAccount.DisplayName));
                content.AppendLine(string.Format((string)"Email = {0}<br/>", (object)model.UserAccount.Email));
                content.AppendLine(string.Format((string)"SupportAccountId = {0}<br/>", supportAccountId));
                content.AppendLine(string.Format((string)"SupportAccountName = {0}<br/>", companyName));

                content.AppendLine(string.Format((string)"Serialnumber/authcode ={0}<br/>", (object)model.DeviceCode));
                content.AppendLine(string.Format((string)"Customerid/Salesordernumber ={0}<br/>", (object)model.CustomerCode));
                SendMail(emailTo, subject, content.ToString(), false, "Internal Email");
            }
        }



        public static void SendMembershipDecisionedEmailToSuperUsers(long supportAccountId, long membershipId, MembershipStatus membershipStatus, long decidingSuperUser)
        {
            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
            var membershipLogic = DependencyResolver.Current.GetService<IMembershipLogic>();
            var userEmail = membershipLogic.GetUserEmailByMembershipId(membershipId);
            var superUser = userAccountLogic.GetUserAccount(decidingSuperUser, decidingSuperUser);

            var userEmails = userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId).Select(u => u.Email).Where(email => !string.IsNullOrEmpty(email.Trim())).ToList();

            if (!userEmails.Any())
            {
                return;
            }

            var companyName = supportAccountLogic.GetSupportAccountNameById(supportAccountId);

            //Get the email template for "Membership Decision"
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(TemplateId.MembershipDecision));

            //Send email only if content & subject is not null
            if (!string.IsNullOrEmpty(emailTemplate.Content) && !string.IsNullOrEmpty(emailTemplate.Subject))
            {
                var subject = string.Format(emailTemplate.Subject, userEmail, companyName);

                //Format the template with values
                var emailBody = string.Format(emailTemplate.Content, userEmail, companyName, membershipStatus.ToString(), superUser.FirstName + " " + superUser.LastName);

                //Queue the notification to AWS
                var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                var response = emailNotificationLogic.QueueEmailNotification(subject, emailBody, userEmails, null, null,
                    decidingSuperUser, userEmails.Count, "Super Users", ApiConstants.MEMBERSHIP_DECISION);

                //Log to elmah if response is blank or no notification Id
                if (response == null || string.IsNullOrEmpty(response.NotificationId))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                    {
                        Source =
                            "EmailHelper.SendMembershipDecisionedEmailToSuperUsers(): Null response from QueueEmailNotification()"
                    });
                }

            }

        }

        public static void SendMembershipApprovalEmail(UserAccountRegistrationEmailModel emailModel, long supportAccountId, long membershipId, string updateMembershipStatusUrl)
        {
            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
            var membershipLogic = DependencyResolver.Current.GetService<IMembershipLogic>();
            var membershipResponseGuid = membershipLogic.GetMembershipResponseGuidBId(membershipId);
            var userEmails = userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId).Select(u => u.Email).Where(email => !string.IsNullOrEmpty(email.Trim())).ToList();

            if (!userEmails.Any())
            {
                return;
            }

            var userName = string.Format("{0} {1} ({2})", emailModel.UserAccount.FirstName, emailModel.UserAccount.LastName, emailModel.UserAccount.Email);
            var companyName = string.Format("{0} (CSP ID: {1})", supportAccountLogic.GetSupportAccountNameById(supportAccountId), supportAccountId);

            updateMembershipStatusUrl = string.Format("{0}/{1}", updateMembershipStatusUrl, membershipResponseGuid);
            var acceptStatusUrl = string.Format("{0}?type=Accept", updateMembershipStatusUrl);
            var rejectStatusUrl = string.Format("{0}?type=Reject", updateMembershipStatusUrl);

            //Get the email template for "Membership Decision Pending"
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(TemplateId.MembershipPendingDecision));

            //Send email only if content & subject is not null
            if (!string.IsNullOrEmpty(emailTemplate.Content) && !string.IsNullOrEmpty(emailTemplate.Subject))
            {
                var subject = string.Format(emailTemplate.Subject, userName, companyName);

                //Format the template with values
                var emailBody = string.Format(emailTemplate.Content, userName, companyName, acceptStatusUrl, rejectStatusUrl);

                //Queue the notification to AWS
                var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                var response = emailNotificationLogic.QueueEmailNotification(subject, emailBody, userEmails, null, null,
                    emailModel.UserAccount.Id == 0 ? 1 : emailModel.UserAccount.Id, userEmails.Count, "Super Users", ApiConstants.MEMBERSHIP_DECISION);

                //Log to elmah if response is blank or no notification Id
                if (response == null || string.IsNullOrEmpty(response.NotificationId))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                    {
                        Source =
                            "EmailHelper.SendMembershipApprovalEmail(): Null response from QueueEmailNotification()"
                    });
                }

            }
        }

        public static void SendPendingMembershipActionedEmailToUser(long membershipId, MembershipStatus status, long decidingSuperUserUserAccountId)
        {
            var membershipLogic = DependencyResolver.Current.GetService<IMembershipLogic>();
            var userEmail = membershipLogic.GetUserEmailByMembershipId(membershipId);
            var companyName = membershipLogic.GetSupportAccountNameByMembershipId(membershipId);
            var statusType = (status == MembershipStatus.Accepted) ? "accepted" : "rejected";

            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
            var superUser = userAccountLogic.GetUserAccount(decidingSuperUserUserAccountId, decidingSuperUserUserAccountId);
            var decidingSuperUser = superUser.FirstName + " " + superUser.LastName;

            var rejectedMessage = string.Empty;
            if (status == MembershipStatus.Rejected)
            {
                rejectedMessage = "Please contact your Super User for additional details.<br/><br/>";
            }

            var subject = string.Format("Your Palo Alto Networks Support Portal for the account {0} has been {1}", companyName, statusType);
            var content = string.Format(@"
                            <BR/>
                            Hello,
                            <BR/><BR/>
                            Your Palo Alto Networks Support Portal for the account {0} has been {1}.
                            <BR/><BR/>{3}
														Thank you,
														Palo Alto Networks", companyName, statusType, decidingSuperUser, rejectedMessage);

            SendMail(userEmail, subject, content, false, "Pending Membership");
        }

        public static void SendWildfireUserActivationEmail(string email, string name, string companyName, string activationCode)
        {
            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();

            var configValues = new ConfigValues();
            var wildfireUserActivationUrl = configValues.GetAppSetting(AppSettings.WildfireUserActivationUrl);
            var wildfireApplicationUrl = configValues.GetAppSetting(AppSettings.WildfireApplicationUrl);

            wildfireUserActivationUrl = string.Format(wildfireUserActivationUrl, activationCode);
            var wildfireJapanUrl = configValues.GetAppSetting(AppSettings.WildfireJapanUrl);

            const string subject = "Palo Alto Networks: New WildFire account created";
            var content = string.Format(@"
                            <BR/>
														Dear {0}:
                            <BR/><BR/>
                            A new WildFire account has been created for you by {1} in the Palo Alto Networks support portal, giving you access to WildFire reports from specific Palo Alto Networks devices.
                            <BR/><BR/>
                            Please log into the Palo Alto Networks login portal and set your password to complete the setup of your account:
                            <br/>
														{2}
                            <br/><br/>
                            Once your account is set up, you may log into WildFire to view the WildFire reports for which you have been granted access.
                            <br/>
                            {3}
                            <BR/><BR/>
                            Alternatively, if you would like to view data in the WildFire Japan cloud, please visit WildFire Japan.
                            <br/>
                            {4}
                            <BR/><BR/>
														Thank you,
														Palo Alto Networks", name, companyName, wildfireUserActivationUrl, wildfireApplicationUrl, wildfireJapanUrl);

            SendMail(email, subject, content, false, "Wildfire User Activation");
        }


        public static void SendMembershipApprovalEmailForAllPending(string updateMembershipStatusUrl)
        {

            var userAccountDataAccess = new UserAccountDataAccess();

            List<object> result = userAccountDataAccess.ReadPendingMembership();

            foreach (var o in result)
            {
                Dictionary<string, string> pendingMembership = (Dictionary<string, string>)o;

                var firstName = pendingMembership["FirstName"];
                var lastName = pendingMembership["LastName"];
                var supportAccountId = Convert.ToInt64(pendingMembership["SupportAccountId"]);
                var companyName = pendingMembership["SupportAccountName"];
                var userName = string.Format("{0} {1}", firstName, lastName);
                var membershipResponseGuid = pendingMembership["MembershipResponseGUID"];

                var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
                var userEmails =
                        userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId)
                                                        .Select(u => u.Email).Where(email => !string.IsNullOrEmpty(email.Trim())).ToList();

                if (userEmails.Any())
                {

                    var adminEmails = string.Join(",", userEmails);
                    var membershipStatusUrl = string.Format("{0}/{1}", updateMembershipStatusUrl, membershipResponseGuid);
                    var acceptStatusUrl = string.Format("{0}?type=Accept", membershipStatusUrl);
                    var rejectStatusUrl = string.Format("{0}?type=Reject", membershipStatusUrl);

                    var subject =
                            string.Format("User {0} requesting access for the account {1} at Palo Alto Networks Support Site",
                                                        userName, companyName);
                    var content = string.Format(@"
                            <BR/>
                            Hello,
                            <BR/><BR/>
                            A new user {0} has requested access for the account {1}
                            <BR/><BR/>
                            To accept the request click on the following link or copy-paste it in your browser:
                            <BR/><BR/>
                            <a href=""{2}"">{2}</a>
                            <BR/><BR/>
                            To reject the request click on the following link or copy-paste it in your browser:
														<BR/><BR/>
                            <a href=""{3}"">{3}</a>
                            <BR/><BR/>
														Thank you,
														Palo Alto Networks", userName, companyName, acceptStatusUrl, rejectStatusUrl);

                    SendMail(adminEmails, subject, content, false, "Membership Decision");
                }
            }
        }

        public static void SendWildfireDeviceRegistrationEmail(string email, string name, List<string> serialNumbers)
        {
            var subject = "Palo Alto Networks: New WildFire data access granted";
            var content = string.Format(@"
                            <BR/>
														Dear {0}:
                            <BR/><BR/>
                            You have been granted access to WildFire reports for additional Palo Alto Networks firewalls by another user. You now have WildFire data visibility for the following devices:
                            <BR/><BR/>
                            {1}
                            <BR/><BR/>
														Thank you,
														Palo Alto Networks", email, String.Join("<BR/>", serialNumbers));

            SendMail(email, subject, content, false, "Wildfire Device Registration");
        }

        public static void SendMfaActivationEmail(string email, string userName, MultiFactorAuthType multiFactorAuthType, bool enabledByAdmin = false)
        {
            var subject = @"2 Factor Authentication Enabled";
            var applicationUrl = ConfigurationManager.AppSettings["ApplicationUrl"];
            var authType = multiFactorAuthType.Equals(MultiFactorAuthType.Email) ? "email" : "Google Authenticator App";

            var content = enabledByAdmin
                            ? @"The Super User on your account has enabled Two-Factor Authentication. Going forward it will be required to use a code that is sent to your 
                                email address in addition to your password to login to the Palo Alto Networks Customer Support Portal.
                                <BR/><BR/>
                                Currently we offer two methods of Two-Factor Authentication: email-based (by default) and Google Authenticator."
                            : string.Format(@"You have selected {0} as your preference for Multi Factor Authentication to the Palo Alto Networks
                                <a href=""{1}"">Customer Support Portal</a>
                                <BR/><BR/>
                                Please use the One Time Passcode(OTP) code in your {2} to login to the Customer Support Portal.",
                                authType, applicationUrl, authType);

            var body = string.Format(@"
                            <BR/>
                            Hello {0},
                            <BR/><BR/>
                            {1}
                            If you would like to change the method used, login to the Customer Support Portal, go to Security Settings on the My Profile page.
                            <BR/><BR/>
                            For more information see this knowledge base article:
                            <BR/>
                            <a href=""https://knowledgebase.paloaltonetworks.com/KCSArticleDetail?id=kA10g000000CmSm"">How to Enable Google Authenticator</a>                            
                            <BR/><BR/>
                            Thank you,
                            <BR/>
							Palo Alto Networks Global Customer Service", userName, content);

            SendMail(email, subject, body, false, "Mfa Activation Email");
        }

        public static void SendMfaDisableEmail(List<string> emails, string accountName, string updatedByUser)
        {
            var configValues = new ConfigValues();
            var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);
            var myProfile = string.Format("{0}/SupportAccount/EditProfile", applicationUrl);

            var subject = @"FOR YOUR INFORMATION: Attempt made to disable Two-Factor Authentication at the Account Level";
            var body = string.Format(@"
                            <BR/>
                            Dear valued Palo Alto Networks customer,
                            <BR/><BR/>
                            This is to inform you that Two-Factor Authentication to log into your Palo Alto Networks Support account  
                            ({0}) has been disabled by {1} . 
                            <BR/><BR/>
                            There is no action required if you prefer to continue using Two-Factor Authentication.Palo Alto Networks encourages 
                            the use of Two-Factor Authentication to increase security assurance of your identity. 
                            If you decide not to use Two-Factor Authentication, you must uncheck the "" Enroll in 2 Factor Authentication""  
                            option in <a href=""{2}"">your profile</a> page.
                            <BR/><BR/>
							Sincerely,
                            <BR/>
							The Palo Alto Networks Global Customer Support Team", accountName, updatedByUser, myProfile);

            SendGridMail(emails, null, null, subject, body, "noreply@auth.paloaltonetworks.com", false);
        }

        // not in use
        private static bool SendSmtpMail(IEnumerable<string> emailTo, string subject, List<Attachment> attachments, string message, bool isEmailOptOutEnabled = true, IEnumerable<string> cc = null)
        {
            try
            {
                var email = new MailMessage();

                email.IsBodyHtml = true;
                email.Priority = MailPriority.High;

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        email.Attachments.Add(attachment);
                    }
                }

                foreach (var to in emailTo)
                {
                    email.To.Add(to);
                }

                email.Subject = subject;
                email.Body = message;
                if (cc != null)
                {
                    foreach (string c in cc)
                    {
                        email.CC.Add(c);
                    }
                }
                // Send the email
                var client = new SmtpClient();
                client.Send(email);
                return true;
            }
            catch (Exception ex)
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }

        }


        public static bool SendMail(string emailTo, string subject, string message, bool isEmailOptOutEnabled = true, string featureName = null)
        {
            return SendGridMail(emailTo, subject, message, isEmailOptOutEnabled, featureName);
        }

        public bool SendGeneralMail(string emailTo, string subject, string message)
        {
            return SendMail(emailTo, subject, message);
        }

        public bool SendGeneralMail(string emailTo, string subject, string message, bool isEmailOptOutEnabled)
        {
            return SendMail(emailTo, subject, message, isEmailOptOutEnabled);
        }

        public static string SendGridMail(List<string> emailTo, List<string> emailCc, List<string> emailBcc, string subject, string body, string emailFrom = "", bool isEmailOptOutEnabled = true, string featureName = null)
        {
            try
            {
                var userEmailCount = 0;

                if (emailTo != null && emailTo.Count > 0)
                {
                    emailTo = GetEnvironmentSpecificToAddresses(emailTo).ToList();
                    userEmailCount += emailTo.Count;
                }
                if (emailCc != null && emailCc.Count > 0)
                {
                    emailCc = GetEnvironmentSpecificToAddresses(emailCc).ToList();
                    userEmailCount += emailCc.Count;
                }
                if (emailBcc != null && emailBcc.Count > 0)
                {
                    emailBcc = GetEnvironmentSpecificToAddresses(emailBcc).ToList();
                    userEmailCount += emailBcc.Count;
                }

                //Queue the notification to AWS
                var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                var response = emailNotificationLogic.QueueEmailNotification(subject, body, emailTo, emailCc, emailBcc, 1, userEmailCount, "Users", string.IsNullOrEmpty(featureName) ? "General" : featureName, isEmailOptOutEnabled);

                //Log to elmah if response is blank or no notification Id
                if (response == null || string.IsNullOrEmpty(response.NotificationId))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                    {
                        Source = "EmailHelper.SendGridMail(): Null response from QueueEmailNotification()"
                    });
                }

                // Comments By LK On 03/25/2019
                // Use ENS instead of SendGrid
                /*
                //set the sender
                message.From = string.IsNullOrEmpty(emailFrom) ? new MailAddress(configValues.GetAppSetting(AppSettings.FromAddressForMails)) 
                                                                : new MailAddress(emailFrom);

                //set the message subject
                message.Subject = subject;

                message.DisableClickTracking();
                //message.DisableOpenTracking();
                message.DisableFooter();

                message.Html = body;

                //create an instance of the SMTP transport mechanism
                var transportInstance = SMTP.GetInstance(new NetworkCredential(configValues.GetAppSetting(AppSettings.SendGridUserName), configValues.GetAppSetting(AppSettings.SendGridPassword)));

                //enable bypass list management
                message.EnableBypassListManagement();

                //send the mail
                transportInstance.Deliver(message);*/
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                SendExceptionEmail(ex);
                return ex.Message;
            }
            return "Success";
        }

        private static bool SendGridMail(string emailTo, string subject, string body, bool isEmailOptOutEnabled = true, string featureName = null)
        {
            //send email only if SendGridEmail is not defined, or defined as "true"
            var sendEmail = ConfigurationManager.AppSettings.Get("SendGridEmail");
            if (!string.IsNullOrEmpty(sendEmail) && sendEmail == "false")
                return true;

            // Comments By LK
            // Need to review the changes
            try
            {
                var emailToList = emailTo.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                SendGridMail(emailToList, null, null, subject, body, string.Empty, isEmailOptOutEnabled, featureName);
                return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                SendExceptionEmail(ex);

                return false;
            }

            // Comments By LK On 03/25/2019
            // Use ENS instead of Smtp
            /*
			try {
				//create a new message object
				var message = SendGrid.GetInstance();

				//set the message recipients
				var tos = GetEnvironmentSpecificToAddresses(emailTo.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
				foreach (var to in tos) {
					message.AddTo(to);
				}

				//set the sender
				var configValues = new ConfigValues();
				message.From = new MailAddress(configValues.GetAppSetting(AppSettings.FromAddressForMails));

				//set the message subject
				message.Subject = subject;

				//message.DisableClickTracking();
				//message.DisableOpenTracking();
				//message.DisableFooter();

				message.Html = body;

				//create an instance of the SMTP transport mechanism
				var transportInstance = SMTP.GetInstance(new NetworkCredential(configValues.GetAppSetting(AppSettings.SendGridUserName), configValues.GetAppSetting(AppSettings.SendGridPassword)));

				//enable bypass list management
				message.EnableBypassListManagement();

				//send the mail
				transportInstance.Deliver(message);

				return true;

			} catch (Exception ex) {

				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				SendExceptionEmail(ex);

				return false;
			}*/
        }

        /// <summary>
        /// Dev and test environments must use a different TO address
        /// </summary>
        /// <param name="tos"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetEnvironmentSpecificToAddresses(IEnumerable<string> tos)
        {
            var configValues = new ConfigValues();

            var testingRegistrationValidationEmail = configValues.GetAppSetting(AppSettings.TestingRegistrationValidationEmail);

            /* 
			 * This is only used if the web config has a test setting in the configuration file. 
			 * If there is no test setting then simply pass the tos parameter through
			 */
            if (string.IsNullOrEmpty(testingRegistrationValidationEmail)) return tos;

            var cspSupportEmailsArray = testingRegistrationValidationEmail.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (cspSupportEmailsArray.Length == 0) throw new ArgumentNullException("AppSettings.TestingRegistrationValidationEmail cannot be empty while testing!");

            return cspSupportEmailsArray;
        }

        public static void SendRequireSuperUserApprovalNotification(long supportAccountId, string firstName, string lastName, long createdByUserAccountId)
        {
            try
            {
                var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
                var configValues = new ConfigValues();
                var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

                //Get Admin/Super Users of the account
                var userEmails = userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId)
                    .Select(u => u.Email)
                    .Where(e => !string.IsNullOrEmpty(e.Trim()))
                    .ToList();

                if (!userEmails.Any())
                {
                    return;
                }

                var companyAccountLink = string.Format("{0}/SupportAccount/ViewAccount/{1}", applicationUrl, supportAccountId);

                //Get the email template for "Require Super Approval Setting"
                var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
                var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(TemplateId.RequireSuperUserApproval));

                //Send email only if content & subject is not null
                if (!string.IsNullOrEmpty(emailTemplate.Content) && !string.IsNullOrEmpty(emailTemplate.Subject))
                {
                    var subject = emailTemplate.Subject;

                    //Format the template with values
                    var emailBody = string.Format(emailTemplate.Content, firstName, lastName, DateTime.Now.ToShortDateString(), companyAccountLink);

                    //Queue the notification to AWS
                    var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                    var response = emailNotificationLogic.QueueEmailNotification(subject, emailBody, userEmails, null, null, createdByUserAccountId, userEmails.Count, "Super Users", ApiConstants.REQUIRES_SU_APPROVAL);

                    //Log to elmah if response is blank or no notification Id
                    if (response == null || string.IsNullOrEmpty(response.NotificationId))
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                        {
                            Source = "EmailHelper.SendRequireSuperUserApprovalNotification(): Null response from QueueEmailNotification()"
                        });
                    }
                }
                else
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                    {
                        Source = "EmailHelper.SendRequireSuperUserApprovalNotification() : Empty Subject or Content in email template for templateId: " + TemplateId.RequireSuperUserApproval
                    });
                }


            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

        }

        /// <summary>
        /// This method sends notification to super users of the support accounts with licensingApi key expiring in 
        /// 2 weeks (14 days) , 1 week (7 days) and 48 hours (2 days)
        /// </summary>
	    public static void SendLicensingKeyExpiryNotification()
        {
            try
            {
                var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
                var configValues = new ConfigValues();
                var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

                var supportAccounts = userAccountLogic.GetSupportAccountsByLicensingApiKeyExpiration();

                if (!supportAccounts.Any())
                    return;

                foreach (var supportAccount in supportAccounts)
                {
                    //Get Admin/Super Users of the account
                    var userEmails = userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccount.SupportAccountId)
                        .Select(u => u.Email)
                        .Where(e => !string.IsNullOrEmpty(e.Trim()))
                        .ToList();

                    if (!userEmails.Any())
                    {
                        return;
                    }

                    var link = string.Format("{0}/SupportAccount/LicensingApi/{1}", applicationUrl, supportAccount.SupportAccountId); ;

                    //set templateId based on days difference
                    var templateId = TemplateId.NotifyExpiryIn14Days;

                    if (supportAccount.Days == 14)
                        templateId = TemplateId.NotifyExpiryIn14Days;
                    else if (supportAccount.Days == 7)
                        templateId = TemplateId.NotifyExpiryIn7Days;
                    else if (supportAccount.Days == 2)
                        templateId = TemplateId.NotifyExpiryIn2Days;

                    //Get the email template for given template id
                    var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
                    var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(templateId));

                    //Send email only if content & subject is not null
                    if (!string.IsNullOrEmpty(emailTemplate.Content) && !string.IsNullOrEmpty(emailTemplate.Subject))
                    {
                        var subject = emailTemplate.Subject;

                        //Format the template with values
                        var emailBody = string.Format(emailTemplate.Content, supportAccount.ExpirationDate.Date, link);

                        //Queue the notification to AWS
                        var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                        var response = emailNotificationLogic.QueueEmailNotification(subject, emailBody, userEmails, null, null, 1,
                            userEmails.Count, "Super Users", ApiConstants.LICENSING_API_EXPIRY, false);

                        //Log to elmah if response is blank or no notification Id
                        if (response == null || string.IsNullOrEmpty(response.NotificationId))
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                            {
                                Source =
                                    "EmailHelper.SendLicensingKeyExpiryNotification(): Null response from QueueEmailNotification()"
                            });
                        }
                    }
                    else
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                        {
                            Source =
                                "EmailHelper.SendLicensingKeyExpiryNotification() : Empty Subject or Content in email template for templateId: " +
                                templateId + " for supportAccountId: " + supportAccount.SupportAccountId
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        /// <summary>
        /// Helper method to send VM ELA related notifications
        /// </summary>
        /// <param name="curUserId"></param>
        /// <param name="destinationEmail"></param>
        /// <param name="grandParentAuthCode"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="featureName"></param>
        /// <param name="grantDate"></param>
        public static void SendVmElaNotification(long curUserId, string destinationEmail, string grandParentAuthCode, long supportAccountId, string featureName, DateTime? grantDate = null)
        {
            var userAccountDataAccess = new UserAccountDataAccess();

            var destinationUser = userAccountDataAccess.ReadUserByEmailId(destinationEmail);
            var sourceUser = userAccountDataAccess.Read(curUserId, curUserId);
            var template = featureName.Equals(ApiConstants.VM_ELA_GRANT_TOKEN_ACCESS) ? TemplateId.GrantToken : TemplateId.AcceptToken;

            //Get the email template for given template id
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(template));
            var emailTo = new List<string> { destinationEmail };

            if (!string.IsNullOrEmpty(emailTemplate.Content))
            {
                if (template.Equals(TemplateId.GrantToken))
                {
                    emailTemplate.Content = string.Format(emailTemplate.Content, destinationUser.FirstName,
                        destinationUser.LastName, sourceUser.FirstName, sourceUser.LastName, grandParentAuthCode);
                }
                else
                {
                    var configValues = new ConfigValues();
                    var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

                    var _supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
                    var companyName = _supportAccountLogic.GetSupportAccountNameById(supportAccountId);

                    var eaDashboardUrl = string.Format("{0}/SupportAccount/ElaSummary/{1}", applicationUrl, supportAccountId);
                    emailTemplate.Content = string.Format(emailTemplate.Content, destinationUser.FirstName,
                        destinationUser.LastName, sourceUser.FirstName, sourceUser.LastName, grandParentAuthCode, grantDate.Value.ToShortDateString(), companyName, eaDashboardUrl);
                }
            }

            QueueEmail(emailTemplate, TemplateId.GrantToken, emailTo, null, null, "ELA Administrators", featureName, supportAccountId);

        }

        /// <summary>
        /// Helper method to send VM ELA Token Usage Notifications
        /// </summary>
        /// <param name="tokenNotificationModel"></param>
        public static void SendVmElaTokenUsageNotification(ElaTokenNotificationModel tokenNotificationModel)
        {
            //Get the email template for given template id
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(TemplateId.TokenUsage));
            var emailTo = new List<string> { tokenNotificationModel.UserEmail };
            var emailBcc = tokenNotificationModel.InternalElaOps;

            if (!string.IsNullOrEmpty(emailTemplate.Content))
            {
                var configValues = new ConfigValues();
                var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

                var enterpriseAgreementUrl = string.Format("{0}/SupportAccount/EA/{1}", applicationUrl, tokenNotificationModel.SupportAccountId);
                emailTemplate.Subject = string.Format(emailTemplate.Subject, tokenNotificationModel.TokensConsumedPercent);
                emailTemplate.Content = string.Format(emailTemplate.Content, tokenNotificationModel.FirstName,
                    tokenNotificationModel.LastName, enterpriseAgreementUrl, tokenNotificationModel.TokensConsumedPercent, tokenNotificationModel.AuthCode);
            }

            QueueEmail(emailTemplate, TemplateId.TokenUsage, emailTo, null, emailBcc, "ELA Administrators",
                        ApiConstants.VM_ELA_TOKEN_USAGE_NOTIFICATION, tokenNotificationModel.SupportAccountId);
        }

        /// <summary>
        /// Helper method to send emails to users transferred from source to destination account
        /// </summary>
        /// <param name="transferredUser"></param>
	    public static void SendUserTransferNotification(UserAccountTransferDetails transferredUser)
        {
            //Get the email template for given template id
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var templateId = transferredUser.IsNewMember ? Convert.ToInt32(TemplateId.NewUserTransfer) : Convert.ToInt32(TemplateId.ExistingUserTransfer);
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(templateId);
            var emailTo = new List<string> { transferredUser.Email };

            if (!string.IsNullOrEmpty(emailTemplate.Subject))
            {
                emailTemplate.Subject = string.Format(emailTemplate.Subject, transferredUser.CloudService);
            }

            if (!string.IsNullOrEmpty(emailTemplate.Content))
            {

                emailTemplate.Content = templateId == Convert.ToInt32(TemplateId.NewUserTransfer) ?
                    string.Format(emailTemplate.Content, transferredUser.FirstName,
                        transferredUser.LastName, transferredUser.CloudService, transferredUser.SourceSupportAccountName,
                        transferredUser.DestinationSupportAccountName, transferredUser.DestinationSupportAccountName, transferredUser.CloudService) :
                string.Format(emailTemplate.Content, transferredUser.FirstName,
                    transferredUser.LastName, transferredUser.CloudService, transferredUser.SourceSupportAccountName,
                    transferredUser.DestinationSupportAccountName, transferredUser.CloudService);
            }

            QueueEmail(emailTemplate, (TemplateId)templateId, emailTo, null, null, string.Format("{0} User", transferredUser.CloudService),
                ApiConstants.USER_TRANSFER_NOTIFICATION, transferredUser.DestinationSupportAccountId);
        }

        /// <summary>
        /// Helper method to send CnSeries ELA related notifications
        /// </summary>
        /// <param name="curUserId"></param>
        /// <param name="destinationEmail"></param>
        /// <param name="grandParentAuthCode"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="featureName"></param>
        /// <param name="grantDate"></param>
        public static void SendCnSeriesElaNotification(long curUserId, string destinationEmail, string grandParentAuthCode, long supportAccountId, string featureName, DateTime? grantDate = null)
        {
            var userAccountDataAccess = new UserAccountDataAccess();

            var destinationUser = userAccountDataAccess.ReadUserByEmailId(destinationEmail);
            var sourceUser = userAccountDataAccess.Read(curUserId, curUserId);
            var template = featureName.Equals(ApiConstants.CN_SN_SERIES_ELA_GRANT_TOKEN_ACCESS) ? TemplateId.GrantToken : TemplateId.AcceptToken;

            //Get the email template for given template id
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(template));
            var emailTo = new List<string> { destinationEmail };

            if (!string.IsNullOrEmpty(emailTemplate.Content))
            {
                if (template.Equals(TemplateId.GrantToken))
                {
                    emailTemplate.Content = string.Format(emailTemplate.Content, destinationUser.FirstName,
                        destinationUser.LastName, sourceUser.FirstName, sourceUser.LastName, grandParentAuthCode);
                }
                else
                {
                    var configValues = new ConfigValues();
                    var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

                    var _supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
                    var companyName = _supportAccountLogic.GetSupportAccountNameById(supportAccountId);

                    var eaDashboardUrl = string.Format("{0}/SupportAccount/ElaSummary/{1}", applicationUrl, supportAccountId);
                    emailTemplate.Content = string.Format(emailTemplate.Content, destinationUser.FirstName,
                        destinationUser.LastName, sourceUser.FirstName, sourceUser.LastName, grandParentAuthCode, grantDate.Value.ToShortDateString(), companyName, eaDashboardUrl);
                }
            }

            QueueEmail(emailTemplate, TemplateId.GrantToken, emailTo, null, null, "ELA Administrators", featureName, supportAccountId);

        }

        /// <summary>
        /// Helper method to send VM ELA Token Usage Notifications
        /// </summary>
        /// <param name="tokenNotificationModel"></param>
        public static void SendCnSeriesElaTokenUsageNotification(ElaTokenNotificationModel tokenNotificationModel)
        {
            //Get the email template for given template id
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(TemplateId.TokenUsage));
            var emailTo = new List<string> { tokenNotificationModel.UserEmail };
            var emailBcc = tokenNotificationModel.InternalElaOps;

            if (!string.IsNullOrEmpty(emailTemplate.Content))
            {
                var configValues = new ConfigValues();
                var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

                var enterpriseAgreementUrl = string.Format("{0}/SupportAccount/EA/{1}", applicationUrl, tokenNotificationModel.SupportAccountId);
                emailTemplate.Subject = string.Format(emailTemplate.Subject, tokenNotificationModel.TokensConsumedPercent);
                emailTemplate.Content = string.Format(emailTemplate.Content, tokenNotificationModel.FirstName,
                    tokenNotificationModel.LastName, enterpriseAgreementUrl, tokenNotificationModel.TokensConsumedPercent, tokenNotificationModel.AuthCode);
            }

            QueueEmail(emailTemplate, TemplateId.TokenUsage, emailTo, null, emailBcc, "ELA Administrators",
                        ApiConstants.CN_SN_SERIES_ELA_TOKEN_USAGE_NOTIFICATION, tokenNotificationModel.SupportAccountId);
        }

        /// <summary>
        /// Helper method to queue and send emails via Email Notification Service
        /// </summary>
        /// <param name="emailTemplate"></param>
        /// <param name="emailTo"></param>
        /// <param name="emailCc"></param>
        /// <param name="emailBcc"></param>
        /// <param name="addressing"></param>
        /// <param name="featureName"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="template"></param>
        public static void QueueEmail(EmailTemplate emailTemplate, TemplateId template, List<string> emailTo, List<string> emailCc,
                                        List<string> emailBcc, string addressing, string featureName, long supportAccountId)
        {
            //Send email only if content & subject is not null
            if (!string.IsNullOrEmpty(emailTemplate.Subject) && !string.IsNullOrEmpty(emailTemplate.Content))
            {
                //Queue the notification to AWS
                var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                var response = emailNotificationLogic.QueueEmailNotification(emailTemplate.Subject, emailTemplate.Content, emailTo, emailCc, emailBcc, 1,
                                                                                emailTo.Count, addressing, featureName);

                //Log to elmah if response is blank or no notification Id
                if (response == null || string.IsNullOrEmpty(response.NotificationId))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                    {
                        Source = string.Format("EmailHelper.QueueEmail() for {0}: Null response from QueueEmailNotification()", featureName)
                    });
                }
            }
            else
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                {
                    Source = string.Format(@"EmailHelper.QueueEmail() for {0}: Empty Subject or Content in email template 
                                            for template id {1} for supportAccountId: {2}.", featureName, template, supportAccountId)
                });
            }
        }

        public static void QueueEmail(int templateId, List<string> emailTo, long supportAccountId, string addressing, string featureName, List<string> emailCc = null, List<string> emailBcc = null, params string[] args)
        {
            //Get the email template for given template id
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(templateId);
            if (args != null)
            {
                emailTemplate.Content = string.Format(emailTemplate.Content, args);
            }

            QueueEmail(emailTemplate, (TemplateId)templateId, emailTo, emailCc, emailBcc, addressing, featureName, supportAccountId);
        }

        /// <summary>
        /// Helper method to send email to set password for thirda party enabled Domain Adminsitrator
        /// </summary>
        /// <param name="transferredUser"></param>
        public static void SendSetPasswordEmail(string email, string fullName, string uniqueId, long supportAccountId)
        {
            //Get the email template for given template id
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var templateId = Convert.ToInt32(TemplateId.SetPassword);
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(templateId);
            var emailTo = new List<string> { email };

            var configValues = new ConfigValues();
            var userActivationUrl = configValues.GetAppSetting(AppSettings.UserActivationUrl);
            var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

            userActivationUrl = string.Format(userActivationUrl, uniqueId);

            if (!string.IsNullOrEmpty(emailTemplate.Subject))
            {
                emailTemplate.Subject = string.Format(emailTemplate.Subject, fullName);
            }

            if (!string.IsNullOrEmpty(emailTemplate.Content))
            {

                emailTemplate.Content = string.Format(emailTemplate.Content, fullName, userActivationUrl, email);
            }

            QueueEmail(emailTemplate, (TemplateId)templateId, emailTo, null, null, "set password",
                ApiConstants.USER_TRANSFER_NOTIFICATION, supportAccountId);
        }
        public static void SendConsentRequestEmailToApprover(long sourceSupportAccountId, long destinationUserAccountId, string consentRequestId, string requestorEmail, long requestorUserAccountId, long targetSupportAccountId)
        {
            var userAccountLogic = DependencyResolver.Current.GetService<IUserAccountLogic>();
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();
            var membershipLogic = DependencyResolver.Current.GetService<IMembershipLogic>();

            var userAccount = userAccountLogic.GetUserAccount(destinationUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);

            if (userAccount == null)
            {
                return;
            }

            var userName = string.Format("{0} {1}", userAccount.FirstName, userAccount.LastName);
            var companyName = supportAccountLogic.GetSupportAccountNameById(sourceSupportAccountId);

            var configValues = new ConfigValues();
            var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);
            string consentRequestUrl = string.Format("{0}/SupportAccount/ConsentRequest/{1}?requestId={2}", applicationUrl, targetSupportAccountId, consentRequestId);

            //Get the email template 
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(Convert.ToInt32(TemplateId.ConsentRequest));

            //Send email only if content & subject is not null
            if (!string.IsNullOrEmpty(emailTemplate.Content) && !string.IsNullOrEmpty(emailTemplate.Subject))
            {
                var subject = string.Format(emailTemplate.Subject, companyName);

                //Format the template with values
                var emailBody = string.Format(emailTemplate.Content, requestorEmail, companyName, consentRequestUrl);

                //Queue the notification to AWS
                var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                var response = emailNotificationLogic.QueueEmailNotification(subject, emailBody, new List<string>() { userAccount.Email }, null, null, requestorUserAccountId, 1, "Domain Administrator", ApiConstants.ConsentRequest);

                //Log to elmah if response is blank or no notification Id
                if (response == null || string.IsNullOrEmpty(response.NotificationId))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                    {
                        Source =
                            "EmailHelper.SendConsentRequestEmailToDomainAdministrator(): Null response from QueueEmailNotification()"
                    });
                }

            }
        }

        public static void SendConsentRequestEmailToRequestor(long targetSupportAccountId, string requestorEmail, string approverEmail, string status, long approverUserAccountId)
        {
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();

            var companyName = supportAccountLogic.GetSupportAccountNameById(targetSupportAccountId);

            //Get the email template 
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();

            int templateId = 0;

            if (status.ToUpper() == ConsentStatus.Approved.ToDescription())
                templateId = Convert.ToInt32(TemplateId.ConsentApproval);

            else if (status.ToUpper() == ConsentStatus.Rejected.ToDescription())
                templateId = Convert.ToInt32(TemplateId.ConsentRejected);

            else if (status.ToUpper() == ConsentStatus.Revoked.ToDescription())
                templateId = Convert.ToInt32(TemplateId.ConsentUnlinked);

            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(templateId);

            //Send email only if content & subject is not null
            if (!string.IsNullOrEmpty(emailTemplate.Content) && !string.IsNullOrEmpty(emailTemplate.Subject))
            {
                var subject = emailTemplate.Subject;

                //Format the template with values
                var emailBody = status.ToUpper() == ConsentStatus.Rejected.ToDescription() ? string.Format(emailTemplate.Content, companyName, approverEmail, DateTime.Now.ToShortDateString(), approverEmail) : string.Format(emailTemplate.Content, companyName, approverEmail, DateTime.Now.ToShortDateString());

                //Queue the notification to AWS
                var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                var response = emailNotificationLogic.QueueEmailNotification(subject, emailBody, new List<string>() { requestorEmail }, null, null, approverUserAccountId, 1, "Consent Request Decision", ApiConstants.ConsentRequest);

                //Log to elmah if response is blank or no notification Id
                if (response == null || string.IsNullOrEmpty(response.NotificationId))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                    {
                        Source =
                            "EmailHelper.SendConsentRequestEmailToDomainAdministrator(): Null response from QueueEmailNotification()"
                    });
                }

            }
        }


        public  static bool SendUpgradeToSuperUserEmail(string emailTo, string firstUserEmail)
        { 
            //Get the email template for "Membership Decision Pending" 
            var emailNotificationDataAccess = DependencyResolver.Current.GetService<IEmailNotificationDataAccess>();
            var emailTemplate = emailNotificationDataAccess.GetEmailTemplateByTemplateId(30);
            string[] emailsList = emailTo.Split(',');

            //Send email only if content & subject is not null 
            if (!string.IsNullOrEmpty(emailTemplate.Content) && !string.IsNullOrEmpty(emailTemplate.Subject))
            {
                var subject = emailTemplate.Subject;
                //Format the template with values
                var emailBody = string.Format(emailTemplate.Content, firstUserEmail);
               
                //Queue the notification to AWS
                var emailNotificationLogic = DependencyResolver.Current.GetService<IEmailNotificationLogic>();
                var response = emailNotificationLogic.QueueEmailNotification(subject, emailBody,emailsList.ToList(), null, null, 1, 1, "AdminCaseElimination", "AdminCaseElimination");

                //Log to elmah if response is blank or no notification Id
                if (response == null || string.IsNullOrEmpty(response.NotificationId))
                {
                    return false;
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception() { Source = "EmailHelper.SendUpgradeToSuperUserEmail(): Null response from QueueEmailNotification()" });
                }
                return true;
            }
            return false;
        }
    }

    public class UserAccountRegistrationEmailModel
    {

        public UserAccount UserAccount { get; set; }

        public string Password { get; set; }

        public string DeviceCode { get; set; }

        public string CustomerCode { get; set; }

        public bool IsTermsAgreed { get; set; }
    }
}
