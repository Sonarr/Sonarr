using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public class ParseErrorReport : ReportBase
    {
        [JsonProperty("t")]
        public string Title { get; set; }
    }
}
