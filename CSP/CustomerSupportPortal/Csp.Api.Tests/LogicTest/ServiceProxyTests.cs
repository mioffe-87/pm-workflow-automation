using Csp.Api.Logic;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Csp.Api.Model;
using Csp.Api.HelperClasses;

namespace Csp.Api.Tests.LogicTest
{
    [TestClass]
    public class ServiceProxyTests
    {
        private readonly ServiceProxy serviceProxy = new ServiceProxy();

        [TestMethod]
        public void GetServiceAccountJwtTest()
        {
            double expiresInSecs;
            var jwt = serviceProxy.CreateServiceAccountJwt("test", out expiresInSecs);
            
            Assert.IsNotNull(jwt);
        }

        [TestMethod]
        public void RequestTest()
        {
            double expiresInSecs;
            var jwt = serviceProxy.CreateServiceAccountJwt("test", out expiresInSecs);

            SupportEvent supportEvent = new SupportEvent() {
                GUID = Guid.NewGuid(),
                Type = SupportEventType.dayOneConfig,
                Data = new SupportEventData() {
                    Email = "test@panw.com",
                    SupportAccountId = 245,
                    UserAccountId = 0,
                    OSVersion = "8.0.0",
                    SerialNumber = "abcde1234",
                    Timestamp = ApiCommon.SecondsSinceUnixEpoch,
                    Status = "Success",
                    TemplateVersion = "v1"
                }
            };

            var response = serviceProxy.Request(
                HttpMethod.Post,
                "https://us-central1-firebase-dev-221719.cloudfunctions.net/supportEventsLogger/",
                "raguilaralej@paloaltonetworks.com",
                supportEvent.ToStringContent(),
                "Bearer " + jwt).Result;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.NoContent);
        }
    }
}
