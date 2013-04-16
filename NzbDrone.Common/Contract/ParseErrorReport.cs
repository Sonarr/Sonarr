using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public class ParseErrorReport : ReportBase
    {
        [JsonProperty("t")]
        public string Title { get; set; }

        protected override Dictionary<string, string> GetString()
        {
            var dic = new Dictionary<string, string>
                          {
                                  {"Title", Title.NullSafe()},
                          };

            return dic;
        }
    }
}
