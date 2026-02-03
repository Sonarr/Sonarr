using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;
using Sonarr.Http;

namespace Sonarr.Api.V5.Series;

[V5ApiController("series")]
public class SeriesFolderController : Controller
{
    private readonly ISeriesService _seriesService;
    private readonly ISeriesTranslationService _seriesTranslationService;
    private readonly IBuildFileNames _fileNameBuilder;

    public SeriesFolderController(ISeriesService seriesService, ISeriesTranslationService seriesTranslationService, IBuildFileNames fileNameBuilder)
    {
        _seriesService = seriesService;
        _seriesTranslationService = seriesTranslationService;
        _fileNameBuilder = fileNameBuilder;
    }

    [HttpGet("{id:int}/folder")]
    [Produces("application/json")]
    public Ok<SeriesFolderResource> GetFolder([FromRoute] int id, [FromQuery] int? language = null)
    {
        var series = _seriesService.GetSeries(id);
        var seriesLanguage = language.HasValue
            ? Language.All.FirstOrDefault(l => l.Id == language.Value) ?? series.Language
            : series.Language;

        var title = series.Title;

        if (seriesLanguage != series.Language)
        {
            var translation = _seriesTranslationService.GetTranslations(id)
                .FirstOrDefault(t => t.Language == seriesLanguage);

            if (translation?.Title.IsNotNullOrWhiteSpace() == true)
            {
                title = translation.Title;
            }
        }

        var folder = _fileNameBuilder.GetSeriesFolder(series, title);

        return TypedResults.Ok(new SeriesFolderResource
        {
            Language = seriesLanguage,
            Folder = folder
        });
    }
}
