using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public HistorySpecification(IHistoryService historyService,
                                    UpgradableSpecification upgradableSpecification,
                                    ICustomFormatCalculationService formatService,
                                    IConfigService configService,
                                    Logger logger)
        {
            _historyService = historyService;
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return DownloadSpecDecision.Accept();
            }

            var cdhEnabled = _configService.EnableCompletedDownloadHandling;
            var qualityProfile = subject.Series.QualityProfile.Value;

            _logger.Debug("Performing history status check on report");

            foreach (var episode in subject.Episodes)
            {
                _logger.Debug("Checking current status of episode [{0}] in history", episode.Id);
                var episodeHistories = _historyService.FindByEpisodeId(episode.Id);

                if (episodeHistories == null || !episodeHistories.Any())
                {
                    continue;
                }

                // Check for grabbed events first
                var rejectionDecision = CheckGrabbedEvents(episodeHistories, subject, qualityProfile, cdhEnabled);
                if (!rejectionDecision.Accepted)
                {
                    return rejectionDecision;
                }

                // Then check for file events if episode has a file
                if (episode.HasFile)
                {
                    rejectionDecision = CheckLastImportedFile(episodeHistories, subject, qualityProfile);
                    if (!rejectionDecision.Accepted)
                    {
                        return rejectionDecision;
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }

        private DownloadSpecDecision CheckGrabbedEvents(List<EpisodeHistory> histories, RemoteEpisode subject, QualityProfile qualityProfile, bool cdhEnabled)
        {
            foreach (var history in histories.Where(h => h.EventType == EpisodeHistoryEventType.Grabbed))
            {
                var customFormats = _formatService.ParseCustomFormat(history, subject.Series);
                var recent = history.Date.After(DateTime.UtcNow.AddHours(-12));

                if (!recent && cdhEnabled)
                {
                    continue;
                }

                var cutoffUnmet = _upgradableSpecification.CutoffNotMet(
                    qualityProfile,
                    history.Quality,
                    customFormats,
                    subject.ParsedEpisodeInfo.Quality);

                var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(
                    qualityProfile,
                    history.Quality,
                    customFormats,
                    subject.ParsedEpisodeInfo.Quality,
                    subject.CustomFormats);

                if (!cutoffUnmet)
                {
                    if (recent)
                    {
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryRecentCutoffMet,
                            "Recent grab event in history already meets cutoff: {0}",
                            history.Quality);
                    }

                    return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCdhDisabledCutoffMet, "CDH is disabled and grab event in history already meets cutoff: {0}", history.Quality);
                }

                var rejectionSubject = recent ? "Recent" : "CDH is disabled and";

                switch (upgradeableRejectReason)
                    {
                        case UpgradeableRejectReason.None:
                            continue;

                        case UpgradeableRejectReason.BetterQuality:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherPreference, "{0} grab event in history is of equal or higher preference: {1}", rejectionSubject, history.Quality);

                        case UpgradeableRejectReason.BetterRevision:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherRevision, "{0} grab event in history is of equal or higher revision: {1}", rejectionSubject, history.Quality.Revision);

                        case UpgradeableRejectReason.QualityCutoff:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCutoffMet, "{0} grab event in history meets quality cutoff: {1}", rejectionSubject, qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                        case UpgradeableRejectReason.CustomFormatCutoff:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatCutoffMet, "{0} grab event in history meets Custom Format cutoff: {1}", rejectionSubject, qualityProfile.CutoffFormatScore);

                        case UpgradeableRejectReason.CustomFormatScore:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScore, "{0} grab event in history has an equal or higher Custom Format score: {1}", rejectionSubject, qualityProfile.CalculateCustomFormatScore(customFormats));

                        case UpgradeableRejectReason.MinCustomFormatScore:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScoreIncrement, "{0} grab event in history has Custom Format score within Custom Format score increment: {1}", rejectionSubject, qualityProfile.MinUpgradeFormatScore);

                        case UpgradeableRejectReason.UpgradesNotAllowed:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryUpgradesNotAllowed, "{0} grab event in history and Quality Profile '{1}' does not allow upgrades", rejectionSubject, qualityProfile.Name);
                    }
            }

            return DownloadSpecDecision.Accept();
        }

        private DownloadSpecDecision CheckLastImportedFile(List<EpisodeHistory> histories, RemoteEpisode subject, QualityProfile qualityProfile)
        {
            var newQuality = subject.ParsedEpisodeInfo.Quality;
            var relevantHistorie = histories.FirstOrDefault(h => h.EventType == EpisodeHistoryEventType.SeriesFolderImported);
            if (relevantHistorie == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var qualityComparer = new QualityModelComparer(qualityProfile);
            var qualityCompare = qualityComparer.Compare(newQuality, relevantHistorie.Quality);
            var historyCustomFormats = _formatService.ParseCustomFormat(relevantHistorie, subject.Series);
            var historyCustomFormatScore = qualityProfile.CalculateCustomFormatScore(historyCustomFormats);

            // New release has better quality - always accept
            if (qualityCompare > 0)
            {
                return DownloadSpecDecision.Accept();
            }

            // New release has worse quality - reject
            if (qualityCompare < 0)
            {
                var reject = DownloadSpecDecision.Reject(DownloadRejectionReason.BetterQuality, "Existing item has better quality, skipping. Existing: {0}. New: {1}", relevantHistorie.Quality, newQuality);
                _logger.Debug(reject.Message);
                return reject;
            }

            // Quality is the same, check custom format score
            if (subject.CustomFormatScore > historyCustomFormatScore)
            {
                // New release has better custom format score
                return DownloadSpecDecision.Accept();
            }
            else
            {
                // New release has same or worse custom format score
                var reject = DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScore,
                    "New item's custom formats [{0}] ({1}) do not improve on [{2}] ({3}), skipping",
                    subject.CustomFormats.ConcatToString(),
                    subject.CustomFormatScore,
                    historyCustomFormats?.ConcatToString(),
                    historyCustomFormatScore);

                _logger.Debug(reject.Message);
                return reject;
            }
        }
    }
}
