using System.Dynamic;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.CustomFilters;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.CustomFilters;

public class CustomFilterResource : RestResource
{
    public string? Type { get; set; }
    public string? Label { get; set; }
    public List<ExpandoObject> Filters { get; set; } = [];
}

public static class CustomFilterResourceMapper
{
    public static CustomFilterResource ToResource(this CustomFilter model)
    {
        return new CustomFilterResource
        {
            Id = model.Id,
            Type = model.Type,
            Label = model.Label,
            Filters = STJson.Deserialize<List<ExpandoObject>>(model.Filters)
        };
    }

    public static CustomFilter ToModel(this CustomFilterResource resource)
    {
        return new CustomFilter
        {
            Id = resource.Id,
            Type = resource.Type,
            Label = resource.Label,
            Filters = STJson.ToJson(resource.Filters)
        };
    }

    public static List<CustomFilterResource> ToResource(this IEnumerable<CustomFilter> filters)
    {
        return filters.Select(ToResource).ToList();
    }
}
