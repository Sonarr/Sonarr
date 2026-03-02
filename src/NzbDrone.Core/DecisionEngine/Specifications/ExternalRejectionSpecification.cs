using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine.ExternalDecisions;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class ExternalRejectionSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IExternalDecisionFactory _externalDecisionFactory;
        private readonly IExternalDecisionStatusService _statusService;
        private readonly Logger _logger;

        public ExternalRejectionSpecification(IExternalDecisionFactory externalDecisionFactory, IExternalDecisionStatusService statusService, Logger logger)
        {
            _externalDecisionFactory = externalDecisionFactory;
            _statusService = statusService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.External;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            var decisions = _externalDecisionFactory.RejectionDecisionsEnabled();

            if (decisions.Empty())
            {
                return DownloadSpecDecision.Accept();
            }

            var seriesTags = subject.Series?.Tags;
            var matchingDecisions = decisions.Where(h =>
            {
                var decisionTags = ((ExternalDecisionDefinition)h.Definition).Tags;

                return decisionTags.Empty() || (seriesTags != null && decisionTags.Intersect(seriesTags).Any());
            }).ToList();

            if (matchingDecisions.Empty())
            {
                return DownloadSpecDecision.Accept();
            }

            var request = BuildRejectionRequest(subject);

            foreach (var decision in matchingDecisions)
            {
                try
                {
                    _logger.Debug("Evaluating external rejection decision '{0}' for '{1}'", decision.Definition.Name, subject.Release.Title);

                    var response = decision.EvaluateRejection(request);

                    _statusService.RecordSuccess(decision.Definition.Id);

                    if (response.Approved)
                    {
                        _logger.Debug("External decision '{0}' approved '{1}'", decision.Definition.Name, subject.Release.Title);
                    }
                    else
                    {
                        var reason = response.Reason.IsNotNullOrWhiteSpace()
                            ? response.Reason
                            : "Rejected by external decision";

                        _logger.Debug("External decision '{0}' rejected '{1}': {2}", decision.Definition.Name, subject.Release.Title, reason);

                        return DownloadSpecDecision.Reject(DownloadRejectionReason.ExternalRejection, "External: {0}", reason);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "External decision '{0}' failed for '{1}', treating as approved (fail-open).", decision.Definition.Name, subject.Release.Title);
                    _statusService.RecordFailure(decision.Definition.Id);
                }
            }

            return DownloadSpecDecision.Accept();
        }

        private ExternalRejectionRequest BuildRejectionRequest(RemoteEpisode subject)
        {
            var release = subject.Release;
            var torrentInfo = release as TorrentInfo;

            return new ExternalRejectionRequest
            {
                DecisionType = nameof(ExternalDecisionType.Rejection),
                Release = new ExternalReleasePayload
                {
                    Guid = release.Guid,
                    Title = release.Title,
                    Indexer = release.Indexer,
                    Quality = subject.ParsedEpisodeInfo?.Quality,
                    CustomFormats = subject.CustomFormats?.Select(cf => new CustomFormatPayload { Id = cf.Id, Name = cf.Name }).ToList() ?? new List<CustomFormatPayload>(),
                    CustomFormatScore = subject.CustomFormatScore,
                    Size = release.Size,
                    Protocol = release.DownloadProtocol.ToString().ToLowerInvariant(),
                    Languages = subject.Languages ?? new List<Languages.Language>(),
                    Seeders = torrentInfo?.Seeders,
                    Peers = torrentInfo?.Peers,
                    Age = release.Age,
                    IndexerPriority = release.IndexerPriority,
                    IndexerFlags = release.IndexerFlags,
                    InfoUrl = release.InfoUrl,
                    InfoHash = torrentInfo?.InfoHash,
                    IsFullSeason = subject.ParsedEpisodeInfo?.FullSeason ?? false,
                    ReleaseType = subject.ParsedEpisodeInfo?.ReleaseType.ToString()
                },
                Series = new ExternalSeriesPayload
                {
                    Id = subject.Series.Id,
                    TvdbId = subject.Series.TvdbId,
                    ImdbId = subject.Series.ImdbId,
                    TmdbId = subject.Series.TmdbId,
                    Title = subject.Series.Title,
                    Year = subject.Series.Year,
                    Status = subject.Series.Status.ToString(),
                    SeriesType = subject.Series.SeriesType.ToString(),
                    Network = subject.Series.Network,
                    Runtime = subject.Series.Runtime,
                    OriginalLanguage = subject.Series.OriginalLanguage,
                    Certification = subject.Series.Certification,
                    Tags = subject.Series.Tags,
                    QualityProfileId = subject.Series.QualityProfileId
                },
                Episodes = subject.Episodes.Select(e => new ExternalEpisodePayload
                {
                    Id = e.Id,
                    SeasonNumber = e.SeasonNumber,
                    EpisodeNumber = e.EpisodeNumber,
                    AbsoluteEpisodeNumber = e.AbsoluteEpisodeNumber,
                    Title = e.Title,
                    AirDate = e.AirDate,
                    AirDateUtc = e.AirDateUtc,
                    Runtime = e.Runtime,
                    HasFile = e.HasFile
                }).ToList(),
                ExistingFiles = subject.Episodes
                    .Where(e => e.EpisodeFileId > 0 && e.EpisodeFile?.Value != null)
                    .Select(e => e.EpisodeFile.Value)
                    .Distinct()
                    .Select(ef => new ExternalExistingFilePayload
                    {
                        Quality = ef.Quality?.Quality?.Name,
                        Size = ef.Size,
                        Languages = ef.Languages ?? new List<Language>(),
                        RelativePath = ef.RelativePath,
                        ReleaseGroup = ef.ReleaseGroup,
                        SceneName = ef.SceneName,
                        DateAdded = ef.DateAdded
                    })
                    .ToList()
            };
        }
    }
}
