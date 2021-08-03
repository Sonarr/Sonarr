using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Update;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Update
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
        public DateTime? InstalledOn { get; set; }
        public bool Installable { get; set; }
        public bool Latest { get; set; }
        public UpdateChanges Changes { get; set; }
        public string Hash { get; set; }
    }

    public static class UpdateResourceMapper
    {
        public static UpdateResource ToResource(this UpdatePackage model)
        {
            if (model == null)
            {
                return null;
            }

            return new UpdateResource
            {
                Version = model.Version,

                Branch = model.Branch,
                ReleaseDate = model.ReleaseDate,
                FileName = model.FileName,
                Url = model.Url,

                //Installed
                //Installable
                //Latest
                Changes = model.Changes,
                Hash = model.Hash,
            };
        }

        public static List<UpdateResource> ToResource(this IEnumerable<UpdatePackage> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
