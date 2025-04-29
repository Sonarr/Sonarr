using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;
using Sonarr.Http;

namespace Sonarr.Api.V5.Series;

[V5ApiController("series")]
public class SeriesFolderController : Controller
{
    private readonly ISeriesService _seriesService;
    private readonly IBuildFileNames _fileNameBuilder;

    public SeriesFolderController(ISeriesService seriesService, IBuildFileNames fileNameBuilder)
    {
        _seriesService = seriesService;
        _fileNameBuilder = fileNameBuilder;
    }

    [HttpGet("{id}/folder")]
    [Produces("application/json")]
    public object GetFolder([FromRoute] int id)
    {
        var series = _seriesService.GetSeries(id);
        var folder = _fileNameBuilder.GetSeriesFolder(series);

        return new
        {
            folder
        };
    }
}
