using System;
using System.IO;
using System.Web;
using Csp.Web.HelperClasses;
using Csp.Api.Model.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csp.Web.Tests.HelperClasses
{
    [TestClass]
    public class JwtTokenManagerTests
    {
        private const string ForgedJwt =
            "eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9" +
            ".eyJzdWIiOiAicm9iYXJAcGFsb2FsdG9uZXR3b3Jrcy5jb20iLCAiYXVkIjogIkNTUCIsICJuYmYiOiAxNzc0OTIwNDU1LCAidXNlckFjY291bnRJZCI6IDYzMjI5MDUyNCwgImlzcyI6ICJDU1AtQVVUSCIsICJleHAiOiAxNzc0OTI0MTE1LCAiaWF0IjogMTc3NDkyMDQ1NSwgImp0aSI6ICI2MzIyOTA1MjRfZm9yZ2VkIn0" +
            ".AAAA";

        private const long MatchingUserAccountId = 632290524;
        private const long MismatchedUserAccountId = 999999999;

        private Func<string, string, string, string, bool> _originalValidationFunc;

        [TestInitialize]
        public void Setup()
        {
            _originalValidationFunc = JwtTokenManager.TokenValidationFunc;
            SetupHttpContext("Bearer " + ForgedJwt);
        }

        [TestCleanup]
        public void Cleanup()
        {
            JwtTokenManager.TokenValidationFunc = _originalValidationFunc;
            HttpContext.Current = null;
        }

        [TestMethod]
        public void IsAuthorized_ForgedJwt_MatchingUid_Rejected()
        {
            JwtTokenManager.TokenValidationFunc = (_, __, ___, ____) => false;

            bool result = JwtTokenManager.IsAuthorized(
                MatchingUserAccountId.ToString(), ClaimType.UserAccountId);

            Assert.IsFalse(result, "Forged JWT with matching userAccountId must be rejected");
        }

        [TestMethod]
        public void IsAuthorized_ForgedJwt_MismatchedUid_Rejected()
        {
            JwtTokenManager.TokenValidationFunc = (_, __, ___, ____) => false;

            bool result = JwtTokenManager.IsAuthorized(
                MismatchedUserAccountId.ToString(), ClaimType.UserAccountId);

            Assert.IsFalse(result, "Forged JWT with mismatched userAccountId must be rejected");
        }

        [TestMethod]
        public void IsAuthorized_ValidationServiceThrows_Rejected()
        {
            JwtTokenManager.TokenValidationFunc = (_, __, ___, ____) =>
            {
                throw new System.Net.WebException("Unauthorized");
            };

            bool result = JwtTokenManager.IsAuthorized(
                MatchingUserAccountId.ToString(), ClaimType.UserAccountId);

            Assert.IsFalse(result, "JWT that causes validation service to throw must be rejected");
        }

        [TestMethod]
        public void IsAuthorized_NoAuthorizationHeader_Rejected()
        {
            SetupHttpContext(null);
            JwtTokenManager.TokenValidationFunc = (_, __, ___, ____) => true;

            bool result = JwtTokenManager.IsAuthorized(
                MatchingUserAccountId.ToString(), ClaimType.UserAccountId);

            Assert.IsFalse(result, "Request with no Authorization header must be rejected");
        }

        [TestMethod]
        public void IsAuthorized_ValidToken_MatchingClaim_Accepted()
        {
            JwtTokenManager.TokenValidationFunc = (_, __, ___, ____) => true;

            bool result = JwtTokenManager.IsAuthorized(
                MatchingUserAccountId.ToString(), ClaimType.UserAccountId);

            Assert.IsTrue(result, "Token validated by auth service with matching claim must be accepted");
        }

        [TestMethod]
        public void IsAuthorized_ValidToken_MismatchedClaim_Rejected()
        {
            JwtTokenManager.TokenValidationFunc = (_, __, ___, ____) => true;

            bool result = JwtTokenManager.IsAuthorized(
                MismatchedUserAccountId.ToString(), ClaimType.UserAccountId);

            Assert.IsFalse(result, "Token validated by auth service with mismatched claim must be rejected");
        }

        private static void SetupHttpContext(string authorizationHeader)
        {
            var request = new HttpRequest("", "http://localhost/api/token/refreshjwttoken", "");
            var response = new HttpResponse(TextWriter.Null);
            HttpContext.Current = new HttpContext(request, response);

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                HttpContext.Current.Request.Headers["Authorization"] = authorizationHeader;
            }
        }
    }
}
