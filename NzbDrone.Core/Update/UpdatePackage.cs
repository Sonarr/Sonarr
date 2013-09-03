using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Update
{
    public class UpdatePackage
    {
        public String Id { get; set; }

        [JsonConverter(typeof(Newtonsoft.Json.Converters.VersionConverter))]
        public Version Version { get; set; }

        public String Branch { get; set; }
        public DateTime ReleaseDate { get; set; }
        public String FileName { get; set; }
        public String Url { get; set; }
    }
}
