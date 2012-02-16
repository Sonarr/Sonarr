using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public class ExceptionReport : ReportBase
    {
        [JsonProperty("t")]
        public string Type { get; set; }
        [JsonProperty("l")]
        public string Logger { get; set; }
        [JsonProperty("lm")]
        public string LogMessage { get; set; }
        [JsonProperty("s")]
        public string String { get; set; }


        protected override Dictionary<string, string> GetString()
        {
            var dic = new Dictionary<string, string>
                          {
                                  {"ExType", Type.NullCheck()},
                                  {"Logger", Logger.NullCheck()},
                                  {"Message", LogMessage.NullCheck()},
                                  {"Str", String.NullCheck()}
                          };

            return dic;
        }
    }

}
