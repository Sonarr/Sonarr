using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Common.Contract
{
    public abstract class ReportBase
    {
        [JsonProperty("v")]
        public string Version { get; set; }

        [JsonProperty("p")]
        public bool IsProduction { get; set; }

        [JsonProperty("u")]
        public Guid UGuid { get; set; }

        public override string ToString()
        {
            var childString = "";
            foreach (var keyValue in GetString())
            {
                childString += string.Format("{0}: {1} ", keyValue.Key, keyValue.Value);
            }

            return string.Format("[{0} Prd:{1} V:{2} ID:{3} | {4}]", GetType().Name, IsProduction, Version, UGuid, childString.Trim());
        }

        protected abstract Dictionary<string,string> GetString();
    }
}
