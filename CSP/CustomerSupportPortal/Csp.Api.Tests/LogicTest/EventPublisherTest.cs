using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Csp.Api.Logic;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Tests.LogicTest
{
    [TestClass]
    public class EventPublisherTest
    {
        [TestMethod]
        public async Task TestPublishEvent()
        {
            //EventPublisher eventPublisher = await EventPublisher.CreateAsync(new EventPublisher.Options()
            //{
            //    ProjectId = "firebase-dev-221719",
            //    TopicId = "events"
            //});

            //var messageId = await eventPublisher.PublishMessage(new EventPublisher.EventMessage()
            //{
            //    Description = "Hello. Bye Bye."
            //});

            //Assert.IsNotNull(messageId, "Failed to publish the event to PubSub service");

        
            AppNotificationData newUserAppNotificationData = new AppNotificationData()
            {
                AccountId = Convert.ToString(245),
                EntityId = Convert.ToString(64657),
                UserId = Convert.ToString(64657)
            };

            AppNotification newUserNotification = new AppNotification()
            {
                AppData = newUserAppNotificationData,
                Category = NotificationCategory.UserRegistration.ToDescription(),
                DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription() },
                Source = NotificationSource.Csp.ToString()
            };

           var messageId= await EventPublisher.PublishEvent(newUserNotification);
           
            //var messageId= await EventPublisher.PublishEvent(superUserEventMessage, eventOptions);
            Assert.IsNotNull(messageId, "Failed to publish the event to PubSub service");
        }
    }
}
