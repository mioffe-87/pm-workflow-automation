using System;
using System.Web.Http;
using System.Collections;
using System.IO;
using System.Web;
using System.Text;
using System.Configuration;
using opentoken;
using Csp.Web.Filters;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Security;
using Csp.Web.HelperClasses;
using System.Net;
using Csp.Api.Model.Enums;
using Newtonsoft.Json.Linq;

namespace Csp.Web.Controllers
{
    public class ProductTrialsController : ApiController
    {
        private readonly IApplicationSettings _applicationSettings;

        public ProductTrialsController(IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        // GET: api/ProductTrials/Products
        [HttpGet]
        [JwtTokenAuthentication]
        public JObject Products()
        {
            var returnData = string.Empty;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(_applicationSettings.ProductsUrl);

                request.Method = "GET";
                request.ContentType = "application/json";
                String username = _applicationSettings.ProductsUserName;
                String password = _applicationSettings.ProductsPassword;
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encoded);

                var response = (HttpWebResponse)request.GetResponse();

                //Preparing the response
                using (var stream = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        returnData = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return JObject.Parse(returnData);
        }


        // POST: api/ProductTrials
        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public JObject Post([FromBody]JObject trialRequestPayload)
        {
            var returnData = string.Empty;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(_applicationSettings.ProductTrialRequestUrl);

                request.Method = "POST";
                request.ContentType = "application/json";
                String username = _applicationSettings.ProductTrialUserName;
                String key = _applicationSettings.ProductTrialKey;
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + key));
                request.Headers.Add("Authorization", "Basic " + encoded);

                //Preparing and Posting th request
                using (var requestStream = request.GetRequestStream())
                {
                    using (var writer = new StreamWriter(requestStream))
                    {
                        writer.Write(trialRequestPayload.ToString());
                        writer.Flush();
                        writer.Dispose();
                    }
                }

                //Preparing response from Request
                using (var responseStream = request.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(responseStream))
                    {
                        returnData = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return JObject.Parse(returnData);
        }

    }
}