using System;
using Newtonsoft.Json;
using NzbDrone.Api.REST;
using NzbDrone.Core.Update;

namespace NzbDrone.Api.Update
{
    public class UpdateResource : RestResource
    {
        [JsonConverter(typeof(Newtonsoft.Json.Converters.VersionConverter))]
        public Version Version { get; set; }

        public String Branch { get; set; }
        public DateTime ReleaseDate { get; set; }
        public String FileName { get; set; }
        public String Url { get; set; }
        public Boolean Installed { get; set; }
        public Boolean Installable { get; set; }
        public Boolean Latest { get; set; }
        public UpdateChanges Changes { get; set; }
        public String Hash { get; set; }
    }
}
