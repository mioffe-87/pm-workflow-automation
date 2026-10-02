using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BotDetect.C5;
using Csp.Web.HelperClasses;
using System.Net;
using System.Web.Http.Results;
using System.Web.Http;

namespace Csp.Web.Extensions
{
    public static class WebExtensions
    {
        /// <summary>
        /// Transform error messages collection into Custom Error Response. The method transforms
        /// array of messages to expected JSON object passed in error response body.
        /// </summary>
        /// <example>
        /// {
        ///   "data": {
        ///     "status": 404,
        ///     "error_message": [{
        ///       "error_message": "Invalid Input: Support Account ID 12345 does not exists."
        ///     }, {
        ///       "error_message": "Invalid Input: User Account ID 0 does not exists."
        ///     }],
        ///     "timestamp": "2022-05-27T11:42:51.941459Z"
        ///   }
        /// }
        /// </example>
        /// <param name="statusCode">Status code: 4xx and 5xx HTML Status Code is expected.</param>
        /// <param name="controller">ASP.NET controller.</param>
        /// <param name="messages">Array of messages to pass in response body "data" object.</param>
        /// <returns>Return NegotiatedContentResult object which is expected by IHttpActionResult.</returns>
        public static NegotiatedContentResult<dynamic> HttpCustomErrorResponse(HttpStatusCode statusCode, 
            ApiController controller, IEnumerable<string> messages)
        {
            return new NegotiatedContentResult<dynamic>(statusCode,
                new
                {
                    data = new
                    {
                        status = (int)statusCode,
                        error_message = (dynamic)messages?.Select(m => new { error_message = m }).ToArray(),
                        timestamp = DateTime.Now.ToString("o")
                    }
                }, controller);
        }

        public static NegotiatedContentResult<T> HttpUnAuthorizedErrorResponse<T>(HttpStatusCode statusCode, T value, ApiController controller)
        {
            return new NegotiatedContentResult<T>(statusCode, value, controller);
        }
    }
}