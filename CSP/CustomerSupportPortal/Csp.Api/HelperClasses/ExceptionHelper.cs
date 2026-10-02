using System.IO;
using System.Net;
using Csp.Api.Model;
using Newtonsoft.Json;

namespace Csp.Api.HelperClasses
{
    public static class ExceptionHelper
    {
        public static CommonApiResponse<dynamic> GetWebException(WebException ex)
        {
            dynamic result = null;

            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
            {
                string jsonResponse = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    return JsonConvert.DeserializeObject<CommonApiResponse<dynamic>>(jsonResponse, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });
                }
            }
            return result;
        }

        public static T GetWebException<T>(WebException ex)
        {
            dynamic result = null;

            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
            {
                string jsonResponse = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    return JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });
                }
            }
            return result;
        }
    }
}
