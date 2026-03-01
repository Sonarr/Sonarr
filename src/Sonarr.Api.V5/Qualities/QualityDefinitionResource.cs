using NzbDrone.Core.Qualities;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Qualities;

public class QualityDefinitionResource : RestResource
{
    public Quality? Quality { get; set; }
    public string? Title { get; set; }
    public int Weight { get; set; }
}

public static class QualityDefinitionResourceMapper
{
    public static QualityDefinitionResource ToResource(this QualityDefinition model)
    {
        return new QualityDefinitionResource
        {
            Id = model.Id,
            Quality = model.Quality,
            Title = model.Title,
            Weight = model.Weight,
        };
    }

    public static QualityDefinition ToModel(this QualityDefinitionResource resource)
    {
        return new QualityDefinition
        {
            Id = resource.Id,
            Quality = resource.Quality,
            Title = resource.Title,
            Weight = resource.Weight,
        };
    }

    public static List<QualityDefinitionResource> ToResource(this IEnumerable<QualityDefinition> models)
    {
        return models.Select(ToResource).ToList();
    }

    public static List<QualityDefinition> ToModel(this IEnumerable<QualityDefinitionResource> resources)
    {
        return resources.Select(ToModel).ToList();
    }
}
