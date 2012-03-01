using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public class ExistingExceptionReport : ReportBase
    {

        [JsonProperty("h")]
        public string Hash { get; set; }

        [JsonProperty("lm")]
        public string LogMessage { get; set; }
        
        protected override Dictionary<string, string> GetString()
        {
            var dic = new Dictionary<string, string>
                          {
                                  {"Message", LogMessage.NullSafe()}
                          };

            return dic;
        }
    }
}