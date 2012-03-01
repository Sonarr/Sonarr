using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public class ExceptionReportResponse 
    {
        [JsonProperty("h")]
        public string ExceptionHash { get; set; }
    }
}