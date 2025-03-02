using NzbDrone.Core.Update;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Update
{
    public class UpdateResource : RestResource
    {
        public required Version Version { get; set; }

        public required string Branch { get; set; }
        public DateTime ReleaseDate { get; set; }
        public required string FileName { get; set; }
        public required string Url { get; set; }
        public bool Installed { get; set; }
        public DateTime? InstalledOn { get; set; }
        public bool Installable { get; set; }
        public bool Latest { get; set; }
        public required UpdateChanges Changes { get; set; }
        public required string Hash { get; set; }
    }

    public static class UpdateResourceMapper
    {
        public static UpdateResource ToResource(this UpdatePackage model)
        {
            return new UpdateResource
            {
                Version = model.Version,

                Branch = model.Branch,
                ReleaseDate = model.ReleaseDate,
                FileName = model.FileName,
                Url = model.Url,

                // Installed
                // Installable
                // Latest
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
