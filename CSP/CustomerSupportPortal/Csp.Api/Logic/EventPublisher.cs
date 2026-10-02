using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace Csp.Api.Logic
{
    public class EventPublisher
    {
       
        private static readonly string _projectId = ConfigurationManager.AppSettings.Get("ProjectId"); 
        private static readonly string _topic = ConfigurationManager.AppSettings.Get("Topic");
        private static readonly string _cspPubSubEventsHost = ConfigurationManager.AppSettings.Get("CspPubSubEventsHost");
        private static readonly string _cspPubSubEventsCertThumbPrint = ConfigurationManager.AppSettings.Get("CspPubSubEventsCertThumbPrint");
        public EventPublisher() { }

        public async Task<string> PublishEvent(AppNotification eventMessage)
        {
            GenericPubSubMessageRequest request = new GenericPubSubMessageRequest()
            {
                message = eventMessage,
                project = _projectId,
                topicName = _topic
            };
            var messageId = await publishMessage(request);
             return messageId; 
        }
        public async Task<string> PublishCustomEvent(AppNotification eventMessage, string topicName)
        {
            GenericPubSubMessageRequest request = new GenericPubSubMessageRequest()
            {
                message = eventMessage,
                project = _projectId,
                topicName = topicName
            };
            return await publishMessage(request);
        }
        public async Task<string> PublishCustomEvent<T>(T eventMessage, string project, string topicName)
        {
            try
            {
                GenericPubSubMessageRequest request = new GenericPubSubMessageRequest() { 
                 message = eventMessage, 
                 project = project,
                 topicName = topicName
                };
                return await publishMessage(request);
            }
            catch (Exception ex) {
                Directory.CreateDirectory(@"C:\logs\FirewallFlex\");
                var logFile = @"C:\logs\FirewallFlex\" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt"; File.AppendAllText(logFile, ex.ToString());
                return string.Empty; 
            }
        }

        public async Task<string> PublishCustomEventNoExceptionHandling<T>(T eventMessage, string project, string topicName)
        {
            GenericPubSubMessageRequest request = new GenericPubSubMessageRequest()
            {
                message = eventMessage,
                project = project,
                topicName = topicName
            };
            return await publishMessage(request);
        }

        public async Task<string> PublishEvent(Events eventMessage)
        {
            GenericPubSubMessageRequest request = new GenericPubSubMessageRequest()
            {
                message = eventMessage,
                project = _projectId,
                topicName = _topic
            };
            var messageId = await publishMessage(request);
            return messageId;
        }

        private async Task<string> publishMessage(GenericPubSubMessageRequest request)
        {
            try
            {
                string baseUrl = string.Format(ApiConstants.publishMessageEndpoint, _cspPubSubEventsHost);
                var jsonPayload = JsonConvert.SerializeObject(request);
                var response = await Task.Run(() => CommonHelper.ProcessWebRequest<GenericPubSubMessageResponse>(baseUrl, HttpMethod.Post.Method, _cspPubSubEventsCertThumbPrint, jsonPayload));

                return string.Empty;
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return string.Empty;

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Directory.CreateDirectory(@"C:\logs\Pubsub\");
                var logFile = @"C:\logs\Pubsub\" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt"; 
                File.AppendAllText(logFile, ex.ToString());
                return string.Empty;
            }
            
        }


    }
}