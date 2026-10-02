using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace Csp.Api.Logic
{
    public class EmailNotificationLogic : IEmailNotificationLogic
    {
        private readonly IEmailNotificationDataAccess _notificationDataAccess;
        private readonly IApplicationSettings _applicationSettings;

        public EmailNotificationLogic(IEmailNotificationDataAccess emailNotificationDataAccess,  IApplicationSettings applicationSettings)
        {
            _notificationDataAccess = emailNotificationDataAccess;
            _applicationSettings = applicationSettings;
        }

        public EmailNotification QueueEmailNotification(string emailsubject, string emailBody, List<string> toList, List<string> ccList, List<string> bccList, 
                                long createdByUserAccountId , long countToBeSent ,string addressing,string featureName, bool isEmailOptOutEnabled = true)
        {

            if (ConfigurationManager.AppSettings["AppName"] != "CSP PROD")
                isEmailOptOutEnabled = false;

            var result = new List<EmailNotification>();

            //Get the From Address
            string mailFrom = ConfigurationManager.AppSettings["FromAddressForMails"];

            //Format the JSON payload for notification microservice Starts
            var awsContent = new List<Content>()
            {
                new Content()
                {
                    Type = "text/html", Value = emailBody 
                }
            };

            var awsFrom = new From() { Email = mailFrom };
            var awsPersonalizations = new List<Personalization>();
            // default to System email
            List<To> emailToList = new List<To>();
            emailToList.Add(new To() { Email = "noreply@paloaltonetworks.com" });    
            List<Bcc> emailBccList = null;

            emailBccList = new List<Bcc>();
            if (toList != null && toList.Any())
            {                
                emailBccList = toList.Select(x => new Bcc() { Email = x }).ToList();
            }

            if (ccList != null && ccList.Any())
            {
                emailBccList.Concat(ccList.Select(x => new Bcc() { Email = x }).ToList());
            }

            if (bccList != null && bccList.Any() )
            {
                emailBccList.Concat(bccList.Select(x => new Bcc() { Email = x }).ToList());               
            }

            awsPersonalizations.Add(new Personalization { To = emailToList, Bcc = emailBccList});

            result.Add(
                    new EmailNotification()
                    {
                        Subject = emailsubject,
                        Content = awsContent,
                        From = awsFrom,
                        Personalizations = awsPersonalizations,
                        IsBlacklistEmailFilterOn = isEmailOptOutEnabled
                    }
            );
            //Formatting Ends

            var response = SendEmailToENS(result);

            //Log to elmah if response is blank or no notification Id
            if (response == null || string.IsNullOrEmpty(response.NotificationId))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception()
                {
                    Source = "EmailNotificationLogic:QueueEmailNotification - Received blank response from Email Notification Service"
                });
            }

            else
            {
                //log to DB with notification id details
                _notificationDataAccess.LogEmailNotificationByNotificationId(new LogEmailNotification()
                {
                    AwsEmailNotificationId = response.NotificationId,
                    ApplicationName = ApiConstants.APPLICATION_NAME,
                    CreatedByUserAccountId = createdByUserAccountId,
                    Status = ApiConstants.PENDING,
                    FeatureName = featureName,
                    CountSent = 0,
                    CountToBeSent = countToBeSent,
                    Addressing = addressing
                });
            }
            return response;
        }

        /// <summary>
        /// Method to update the notification status for all pending notifications
        /// </summary>
        public void UpdatePendingEmailNotificationStatus()
        {
            try
            {
               //Get the list of pending notification ids
               var list= _notificationDataAccess.GetPendingEmailNotificationsList();

                if (list.Count > 0)
                {
                    foreach (string notficationId in list)
                    {
                        //Get the status of the batch from the service
                        EmailNotification logresult = GetStatusByAwsNotificationId(notficationId);

                        if (logresult != null)
                        {
                            
                            if (logresult.TotalBatchCount == logresult.CompletedBatchCount)
                            {
                                //update logemailnotificationresult
                               _notificationDataAccess.UpdateEmailNotificationStatus(logresult.CompletedRecipientCount,notficationId);
                            }
                           
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        /// <summary>
        /// Method to invoke AWS Email Notification Service
        /// </summary>
        /// <param name="emailNotifications">List&lt;EmailNotification&gt;</param>
        /// <returns>EmailNotification</returns>
        private EmailNotification SendEmailToENS(List<EmailNotification> emailNotifications)
        {
            EmailNotification ensResult = null;
            var url = ConfigurationManager.AppSettings["ENSUrl"].Trim();
            var notificationEmailApplicationId = ConfigurationManager.AppSettings["ENSClientId"].Trim();

            var token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                var bearerAccesstoken = "Bearer" + " " + token;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("x-application-id", notificationEmailApplicationId);
                httpWebRequest.Headers.Add("x-request-body-encoded", "true");
                httpWebRequest.Headers.Add("x-content-encoding", "gzip");
                httpWebRequest.Headers.Add("Authorization", bearerAccesstoken);
                httpWebRequest.ContentType = "application/json";

                try
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        //Convert to JSON
                        string emailNotificationJson = JsonConvert.SerializeObject(emailNotifications[0]);

                        //Compress the payload
                        var compressedDataString = ZipString(emailNotificationJson);

                        streamWriter.Write(compressedDataString);
                        streamWriter.Flush();
                        streamWriter.Close();

                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            EmailNotificationResponse ensResponse = JsonConvert.DeserializeObject<EmailNotificationResponse>(result, new JsonSerializerSettings()
                            {
                                MaxDepth = 32,
                                TypeNameHandling = TypeNameHandling.None
                            });
                            ensResult = ensResponse.EmailNotification;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(JsonConvert.SerializeObject(httpWebRequest)));
                }
            }
            
            return ensResult;
        }

        private static string ZipString(string text)
        {
            var arra = GZip(text);
            return Convert.ToBase64String(arra);

        }

        private static byte[] GZip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyToBytes(msi, gs, bytes.Length * 20);
                }

                return mso.ToArray();
            }
        }

        private static void CopyToBytes(Stream src, Stream dest, int len)
        {
            byte[] bytes = new byte[len];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        /// <summary>
        /// Method to get status of notification batch by notification id
        /// </summary>
        /// <param name="notificationId">string</param>
        /// <param name="reuseToken">bool</param>
        /// <returns>EmailNotification</returns>
        private EmailNotification GetStatusByAwsNotificationId(string notificationId)
        {
            var notificationUniqueId = string.Empty;
            EmailNotification awsEmailResult = null;
            var url = ConfigurationManager.AppSettings["ENSUrl"].ToString().Trim();
            var notificationEmailApplicationId = ConfigurationManager.AppSettings["ENSClientId"].ToString().Trim();
            var awsLogsUrl = string.Format(string.Concat(url, "/v2/", "{0}","/logs"), notificationId);

            var token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                 var accessToken = "Bearer" + " " + token;
                 var httpWebRequest = (HttpWebRequest)WebRequest.Create(awsLogsUrl);
                 httpWebRequest.ContentType = "application/json";
                 httpWebRequest.Method = "GET";
                 httpWebRequest.Headers.Add("x-application-id", notificationEmailApplicationId);
                 httpWebRequest.Headers.Add("Authorization", accessToken);
                
                try
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        EmailNotificationResponse ensResponse = JsonConvert.DeserializeObject<EmailNotificationResponse>(result, new JsonSerializerSettings()
                        {
                            MaxDepth = 32,
                            TypeNameHandling = TypeNameHandling.None
                        });
                        awsEmailResult = ensResponse.EmailNotification;
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(JsonConvert.SerializeObject(httpWebRequest)));
                }

            }
           
            return awsEmailResult;
        }

        private string GetAuthToken()
        {
            var token = string.Empty;

            //Cache the token
            object cachedData = HttpRuntime.Cache.Get("ENSauthToken");
            if (cachedData != null)
                return (string)cachedData;
            try
            {
                var authorizationUrl = ConfigurationManager.AppSettings["ENSAuthTokenURL"].Trim();
                var clientId = ConfigurationManager.AppSettings["client_Id"].Trim();
                var secret = ConfigurationManager.AppSettings["secret"].Trim();

                authorizationUrl = string.Format(authorizationUrl, clientId, secret);

                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(authorizationUrl);

                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var jsonResponse = string.Empty;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    jsonResponse = streamReader.ReadToEnd();
                }

                dynamic jsonToken = JsonConvert.DeserializeObject(jsonResponse, new JsonSerializerSettings()
                {
                    MaxDepth = 32,
                    TypeNameHandling = TypeNameHandling.None
                });
                
                if (jsonToken != null && ((JContainer)jsonToken).Count > 0 )
                {
                    token = jsonToken.access_token.Value;
                    //actual token expires in 3600 sec so expire the cache 60s before
                    HttpRuntime.Cache.Insert("ENSauthToken", token, null, DateTime.Now.AddSeconds(3540), Cache.NoSlidingExpiration);
                }
               
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error occured in GetAuthToken"));
            }
            return token;
        }
    }
}
