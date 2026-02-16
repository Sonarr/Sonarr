using NzbDrone.Core.Profiles.Releases;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Profiles.Release;

public class ReleaseProfileResource : RestResource
{
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public List<string> Required { get; set; } = [];
    public List<string> Ignored { get; set; } = [];
    public bool AirDateRestriction { get; set; }
    public int AirDateGracePeriod { get; set; }
    public List<int> IndexerIds { get; set; } = [];
    public HashSet<int> Tags { get; set; } = [];
    public HashSet<int> ExcludedTags { get; set; } = [];
}

public static class RestrictionResourceMapper
{
    public static ReleaseProfileResource ToResource(this ReleaseProfile model)
    {
        return new ReleaseProfileResource
        {
            Id = model.Id,
            Name = model.Name,
            Enabled = model.Enabled,
            Required = model.Required ?? [],
            Ignored = model.Ignored ?? [],
            AirDateRestriction = model.AirDateRestriction,
            AirDateGracePeriod = model.AirDateGracePeriod,
            IndexerIds = model.IndexerIds ?? [],
            Tags = model.Tags ?? [],
            ExcludedTags = model.ExcludedTags ?? [],
        };
    }

    public static ReleaseProfile ToModel(this ReleaseProfileResource resource)
    {
        return new ReleaseProfile
        {
            Id = resource.Id,
            Name = resource.Name,
            Enabled = resource.Enabled,
            Required = resource.Required,
            Ignored = resource.Ignored,
            AirDateRestriction = resource.AirDateRestriction,
            AirDateGracePeriod = resource.AirDateGracePeriod,
            IndexerIds = resource.IndexerIds,
            Tags = resource.Tags,
            ExcludedTags = resource.ExcludedTags
        };
    }

    public static List<ReleaseProfileResource> ToResource(this IEnumerable<ReleaseProfile> models)
    {
        return models.Select(ToResource).ToList();
    }
}
