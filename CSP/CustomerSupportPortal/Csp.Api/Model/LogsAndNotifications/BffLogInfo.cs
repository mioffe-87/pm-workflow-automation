using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.LogsAndNotifications
{
    /// <summary>
    /// 
    /// <code>
    /// {
    ///   "source": "SAP_TEST_Entitlement BFF",
    ///   "log": "{ \"sapId\": 123, \"message\": \"Sample message to log.\" }",
    ///   "logLevel": "3",
    ///   "timeStamp":"2022-07-22T11:42:51.941459Z"
    /// }
    /// </code>
    /// </summary>
    public class BffLogInfo
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("log")]
        public string Log { get; set; }

        [JsonProperty("logLevel")]
        public string LogLevel
        {
            get
            {
                return ((int)BffLogLevel).ToString();
            }
            set
            {
                EBffLogLevel bffLogLevel = EBffLogLevel.NONE;
                if (Enum.TryParse<EBffLogLevel>(value, out bffLogLevel))
                {
                    BffLogLevel = bffLogLevel;
                }
            }
        }

        [JsonIgnore]
        public EBffLogLevel BffLogLevel { get; set; }

        [JsonIgnore]
        public DateTime? Date { get; set; }

        [JsonProperty("timeStamp")]
        public string TimeStamp { get { return Date?.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ") ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"); } set { } }
    }
}
