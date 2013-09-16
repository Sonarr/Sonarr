using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Update
{
    public class UpdatePackage
    {
        public string Id { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        public String Branch { get; set; }
        public DateTime ReleaseDate { get; set; }
        public String FileName { get; set; }
        public String Url { get; set; }
    }
}
