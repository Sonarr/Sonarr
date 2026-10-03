using NzbDrone.Core.Languages;
using NzbDrone.Core.Tv;

namespace Sonarr.Api.V5.Series
{
    public class SeriesTranslationResource
    {
        public Language? Language { get; set; }
        public string? Title { get; set; }
        public string? Overview { get; set; }
    }

    public static class SeriesTranslationResourceMapper
    {
        public static SeriesTranslationResource ToResource(this SeriesTranslation model)
        {
            return new SeriesTranslationResource
            {
                Language = model.Language,
                Title = model.Title,
                Overview = model.Overview
            };
        }

        public static SeriesTranslation ToModel(this SeriesTranslationResource resource)
        {
            return new SeriesTranslation
            {
                Language = resource.Language,
                Title = resource.Title,
                Overview = resource.Overview
            };
        }

        public static List<SeriesTranslationResource> ToResource(this IEnumerable<SeriesTranslation> models)
        {
            return models.Select(ToResource).ToList();
        }

        public static List<SeriesTranslation> ToModel(this IEnumerable<SeriesTranslationResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
