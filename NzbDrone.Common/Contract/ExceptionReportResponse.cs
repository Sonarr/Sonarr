using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public class ExceptionReportResponse 
    {
        [JsonProperty("id")]
        public int ExceptionId { get; set; }
    }
}