using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public interface IExternalPrioritizationService
    {
        void PopulateExternalPriorityScores(List<DownloadDecision> decisions);
    }

    public class ExternalPrioritizationService : IExternalPrioritizationService
    {
        private readonly IExternalDecisionFactory _externalDecisionFactory;
        private readonly IExternalDecisionStatusService _statusService;
        private readonly Logger _logger;

        public ExternalPrioritizationService(IExternalDecisionFactory externalDecisionFactory, IExternalDecisionStatusService statusService, Logger logger)
        {
            _externalDecisionFactory = externalDecisionFactory;
            _statusService = statusService;
            _logger = logger;
        }

        public void PopulateExternalPriorityScores(List<DownloadDecision> decisions)
        {
            var externalDecisions = _externalDecisionFactory.PrioritizationDecisionsEnabled();

            if (externalDecisions.Empty())
            {
                return;
            }

            var withSeries = decisions.Where(d => d.RemoteEpisode.Series != null).ToList();

            var seriesGroups = withSeries.GroupBy(d => d.RemoteEpisode.Series.Id).ToList();

            foreach (var group in seriesGroups)
            {
                var groupDecisions = group.ToList();

                foreach (var externalDecision in externalDecisions)
                {
                    ApplyDecisionScores(externalDecision, groupDecisions);
                }
            }
        }

        private void ApplyDecisionScores(IExternalDecision externalDecision, List<DownloadDecision> decisions)
        {
            if (decisions.Empty())
            {
                return;
            }

            var firstDecision = decisions.First();
            var series = firstDecision.RemoteEpisode.Series;

            var decisionTags = ((ExternalDecisionDefinition)externalDecision.Definition).Tags;

            if (decisionTags is { Count: > 0 })
            {
                var seriesTags = series?.Tags;

                if (seriesTags is not { Count: > 0 } || !decisionTags.Intersect(seriesTags).Any())
                {
                    return;
                }
            }

            try
            {
                var request = BuildPrioritizationRequest(decisions);

                _logger.Debug("Evaluating external prioritization decision '{0}' for series '{1}' with {2} releases.", externalDecision.Definition.Name, series.Title, decisions.Count);

                var response = externalDecision.EvaluatePrioritization(request);

                if (response?.Scores == null || response.Scores.Count == 0)
                {
                    _logger.Debug("External prioritization decision '{0}' returned empty response for series '{1}', keeping default scores.", externalDecision.Definition.Name, series.Title);
                }
                else
                {
                    AssignScores(decisions, response.Scores);

                    _logger.Debug("External prioritization decision '{0}' assigned priority scores for {1} releases for series '{2}'.", externalDecision.Definition.Name, decisions.Count, series.Title);
                }

                _statusService.RecordSuccess(externalDecision.Definition.Id);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "External prioritization decision '{0}' failed for series '{1}', keeping current scores (fail-open).", externalDecision.Definition.Name, series.Title);
                _statusService.RecordFailure(externalDecision.Definition.Id);
            }
        }

        private void AssignScores(List<DownloadDecision> decisions, Dictionary<string, int> scores)
        {
            foreach (var decision in decisions)
            {
                var guid = decision.RemoteEpisode.Release.Guid;

                decision.RemoteEpisode.ExternalPriorityScore = scores.TryGetValue(guid, out var score) ? score : 0;
            }
        }

        private ExternalPrioritizationRequest BuildPrioritizationRequest(List<DownloadDecision> decisions)
        {
            var firstDecision = decisions.First();
            var series = firstDecision.RemoteEpisode.Series;

            var allEpisodes = decisions.SelectMany(d => d.RemoteEpisode.Episodes)
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .ToList();

            return new ExternalPrioritizationRequest
            {
                DecisionType = nameof(ExternalDecisionType.Prioritization),
                Releases = decisions.Select(d =>
                {
                    var release = d.RemoteEpisode.Release;
                    var torrentInfo = release as TorrentInfo;

                    return new ExternalReleasePayload
                    {
                        Guid = release.Guid,
                        Title = release.Title,
                        Indexer = release.Indexer,
                        Quality = d.RemoteEpisode.ParsedEpisodeInfo?.Quality,
                        CustomFormats = d.RemoteEpisode.CustomFormats?.Select(cf => new CustomFormatPayload { Id = cf.Id, Name = cf.Name }).ToList() ?? new List<CustomFormatPayload>(),
                        CustomFormatScore = d.RemoteEpisode.CustomFormatScore,
                        Size = release.Size,
                        Protocol = release.DownloadProtocol.ToString().ToLowerInvariant(),
                        Languages = d.RemoteEpisode.Languages ?? new List<Language>(),
                        Seeders = torrentInfo?.Seeders,
                        Peers = torrentInfo?.Peers,
                        Age = release.Age,
                        IndexerPriority = release.IndexerPriority,
                        IndexerFlags = release.IndexerFlags,
                        InfoUrl = release.InfoUrl,
                        InfoHash = torrentInfo?.InfoHash,
                        IsFullSeason = d.RemoteEpisode.ParsedEpisodeInfo?.FullSeason ?? false,
                        ReleaseType = d.RemoteEpisode.ParsedEpisodeInfo?.ReleaseType.ToString()
                    };
                }).ToList(),

                Series = new ExternalSeriesPayload
                {
                    Id = series.Id,
                    TvdbId = series.TvdbId,
                    ImdbId = series.ImdbId,
                    TmdbId = series.TmdbId,
                    Title = series.Title,
                    Year = series.Year,
                    Status = series.Status.ToString(),
                    SeriesType = series.SeriesType.ToString(),
                    Network = series.Network,
                    Runtime = series.Runtime,
                    OriginalLanguage = series.OriginalLanguage,
                    Certification = series.Certification,
                    Tags = series.Tags,
                    QualityProfileId = series.QualityProfileId
                },

                Episodes = allEpisodes.Select(e => new ExternalEpisodePayload
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
                }).ToList()
            };
        }
    }
}
