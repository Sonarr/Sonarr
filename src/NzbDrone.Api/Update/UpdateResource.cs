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

        public string Branch { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public bool Installed { get; set; }
        public bool Installable { get; set; }
        public bool Latest { get; set; }
        public UpdateChanges Changes { get; set; }
        public string Hash { get; set; }
    }
}
