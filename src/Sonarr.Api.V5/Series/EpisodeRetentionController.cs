using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Tv;
using Sonarr.Http;

namespace Sonarr.Api.V5.Series;

[V5ApiController("series")]
public class EpisodeRetentionController : Controller
{
    private readonly IEpisodeRetentionService _retentionService;

    public EpisodeRetentionController(IEpisodeRetentionService retentionService)
    {
        _retentionService = retentionService;
    }

    [HttpGet("{id:int}/retention")]
    [Produces("application/json")]
    public Results<Ok<EpisodeRetentionResource>, NotFound> Preview(int id)
    {
        try
        {
            return TypedResults.Ok(EpisodeRetentionResource.FromPlan(_retentionService.Preview(id)));
        }
        catch (ModelNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}

public class EpisodeRetentionResource
{
    public List<RetentionFileResource> Files { get; set; } = [];
    public List<RetentionEpisodeResource> UnmonitorEpisodes { get; set; } = [];

    public static EpisodeRetentionResource FromPlan(EpisodeRetentionPlan plan)
    {
        var episodesByFileId = plan.Episodes.ToLookup(episode => episode.EpisodeFileId);

        return new EpisodeRetentionResource
        {
            Files = plan.FilesToDelete.Select(file => new RetentionFileResource
            {
                Id = file.Id,
                RelativePath = file.RelativePath,
                Episodes = episodesByFileId[file.Id]
                    .Select(RetentionEpisodeResource.FromEpisode).ToList()
            }).ToList(),
            UnmonitorEpisodes = plan.OlderMissingEpisodesToUnmonitor.Select(RetentionEpisodeResource.FromEpisode).ToList()
        };
    }
}

public class RetentionFileResource
{
    public int Id { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public List<RetentionEpisodeResource> Episodes { get; set; } = [];
}

public class RetentionEpisodeResource
{
    public int Id { get; set; }
    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;

    public static RetentionEpisodeResource FromEpisode(Episode episode)
    {
        return new RetentionEpisodeResource
        {
            Id = episode.Id,
            SeasonNumber = episode.SeasonNumber,
            EpisodeNumber = episode.EpisodeNumber,
            Title = episode.Title
        };
    }
}
