using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Release;

[V5ApiController("release/push")]
public class ReleasePushController : RestController<ReleasePushResource>
{
    private readonly IMakeDownloadDecision _downloadDecisionMaker;
    private readonly IProcessDownloadDecisions _downloadDecisionProcessor;
    private readonly IIndexerFactory _indexerFactory;
    private readonly IDownloadClientFactory _downloadClientFactory;
    private readonly Logger _logger;

    private readonly QualityProfile _qualityProfile;

    private static readonly object PushLock = new object();

    public ReleasePushController(IMakeDownloadDecision downloadDecisionMaker,
                             IProcessDownloadDecisions downloadDecisionProcessor,
                             IIndexerFactory indexerFactory,
                             IDownloadClientFactory downloadClientFactory,
                             IQualityProfileService qualityProfileService,
                             Logger logger)
    {
        _downloadDecisionMaker = downloadDecisionMaker;
        _downloadDecisionProcessor = downloadDecisionProcessor;
        _indexerFactory = indexerFactory;
        _downloadClientFactory = downloadClientFactory;
        _logger = logger;

        _qualityProfile = qualityProfileService.GetDefaultProfile(string.Empty);

        PostValidator.RuleFor(s => s.Title).NotEmpty();
        PostValidator.RuleFor(s => s.DownloadUrl).NotEmpty().When(s => s.MagnetUrl.IsNullOrWhiteSpace());
        PostValidator.RuleFor(s => s.MagnetUrl).NotEmpty().When(s => s.DownloadUrl.IsNullOrWhiteSpace());
        PostValidator.RuleFor(s => s.Protocol).NotEmpty();
        PostValidator.RuleFor(s => s.PublishDate).NotEmpty();
    }

    [HttpPost]
    [Consumes("application/json")]
    public ActionResult<ReleaseResource> Create([FromBody] ReleasePushResource release)
    {
        _logger.Info("Release pushed: {0} - {1}", release.Title, release.DownloadUrl ?? release.MagnetUrl);

        ValidateResource(release);

        var info = release.ToModel();

        info.Guid = "PUSH-" + info.DownloadUrl;

        ResolveIndexer(info);

        var downloadClientId = ResolveDownloadClientId(release);

        DownloadDecision? decision;

        lock (PushLock)
        {
            var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info }, true);

            decision = decisions.FirstOrDefault();

            _downloadDecisionProcessor.ProcessDecision(decision, downloadClientId).GetAwaiter().GetResult();
        }

        if (decision?.RemoteEpisode.ParsedEpisodeInfo == null)
        {
            throw new ValidationException(new List<ValidationFailure> { new("Title", "Unable to parse", release.Title) });
        }

        return decision.MapDecision(1, _qualityProfile);
    }

    private void ResolveIndexer(ReleaseInfo release)
    {
        var indexer = _indexerFactory.ResolveIndexer(release.IndexerId, release.Indexer);

        if (indexer == null)
        {
            _logger.Debug("Push Release {0} not associated with an indexer.", release.Title);
        }
        else
        {
            _logger.Debug("Push Release {0} associated with indexer '{1} ({2})", release.Title, indexer.Name, indexer.Id);

            release.IndexerId = indexer.Id;
            release.Indexer = indexer.Name;
        }
    }

    private int? ResolveDownloadClientId(ReleasePushResource release)
    {
        var downloadClient = _downloadClientFactory.ResolveDownloadClient(release.DownloadClientId, release.DownloadClientName);

        if (downloadClient == null)
        {
            _logger.Debug("Push Release {0} not associated with a download client.", release.Title);
        }
        else
        {
            _logger.Debug("Push Release {0} associated with download client '{1} ({2})", release.Title, downloadClient.Name, downloadClient.Id);
        }

        return downloadClient?.Id;
    }
}
