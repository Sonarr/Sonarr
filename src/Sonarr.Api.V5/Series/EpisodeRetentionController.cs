using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using Sonarr.Http;
using BadRequestException = Sonarr.Http.REST.BadRequestException;

namespace Sonarr.Api.V5.Series;

[V5ApiController("series")]
public class EpisodeRetentionController : Controller
{
    private readonly IEpisodeRetentionService _retentionService;
    private readonly IManageCommandQueue _commandQueue;

    public EpisodeRetentionController(IEpisodeRetentionService retentionService, IManageCommandQueue commandQueue)
    {
        _retentionService = retentionService;
        _commandQueue = commandQueue;
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

    [HttpPost("{id:int}/retention")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public Results<Accepted<EpisodeRetentionQueuedResource>, NotFound> Trim(int id, [FromBody] EpisodeRetentionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PreviewToken))
        {
            throw new BadRequestException("Preview cleanup before trimming episodes.");
        }

        try
        {
            var plan = _retentionService.Preview(id);

            if (!plan.Enabled || !string.Equals(plan.PreviewToken, request.PreviewToken, StringComparison.Ordinal))
            {
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Episode retention changed. Preview cleanup again.");
            }

            var command = _commandQueue.Push(new EpisodeRetentionCommand
            {
                SeriesId = id,
                PreviewToken = request.PreviewToken,
                SendUpdatesToClient = true
            },
                trigger: CommandTrigger.Manual);

            return TypedResults.Accepted($"/api/v5/command/{command.Id}", new EpisodeRetentionQueuedResource { CommandId = command.Id });
        }
        catch (ModelNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}

public class EpisodeRetentionRequest
{
    public string PreviewToken { get; set; } = string.Empty;
}

public class EpisodeRetentionQueuedResource
{
    public int CommandId { get; set; }
}

public class EpisodeRetentionResource
{
    public string PreviewToken { get; set; } = string.Empty;
    public List<RetentionFileResource> Files { get; set; } = [];
    public List<RetentionEpisodeResource> UnmonitorEpisodes { get; set; } = [];

    public static EpisodeRetentionResource FromPlan(EpisodeRetentionPlan plan)
    {
        var episodesByFileId = plan.Episodes.ToLookup(episode => episode.EpisodeFileId);

        return new EpisodeRetentionResource
        {
            PreviewToken = plan.PreviewToken,
            Files = plan.FilesToDelete.Select(file => new RetentionFileResource
            {
                Id = file.Id,
                RelativePath = file.RelativePath,
                Episodes = episodesByFileId[file.Id]
                    .Select(RetentionEpisodeResource.FromEpisode).ToList()
            }).ToList(),
            UnmonitorEpisodes = plan.EpisodesToUnmonitor.Select(RetentionEpisodeResource.FromEpisode).ToList()
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
