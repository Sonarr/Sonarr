using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

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

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
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
                var mostRecent = _historyService.MostRecentForEpisode(episode.Id);

                if (mostRecent != null && mostRecent.EventType == EpisodeHistoryEventType.Grabbed)
                {
                    var recent = mostRecent.Date.After(DateTime.UtcNow.AddHours(-12));

                    if (!recent && cdhEnabled)
                    {
                        continue;
                    }

                    var customFormats = _formatService.ParseCustomFormat(mostRecent, subject.Series);

                    // The series will be the same as the one in history since it's the same episode.
                    // Instead of fetching the series from the DB reuse the known series.
                    var cutoffUnmet = _upgradableSpecification.CutoffNotMet(
                        subject.Series.QualityProfile,
                        mostRecent.Quality,
                        customFormats,
                        subject.ParsedEpisodeInfo.Quality);

                    var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(
                        subject.Series.QualityProfile,
                        mostRecent.Quality,
                        customFormats,
                        subject.ParsedEpisodeInfo.Quality,
                        subject.CustomFormats);

                    if (!cutoffUnmet)
                    {
                        if (recent)
                        {
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryRecentCutoffMet, "Recent grab event in history already meets cutoff: {0}", mostRecent.Quality);
                        }

                        return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCdhDisabledCutoffMet, "CDH is disabled and grab event in history already meets cutoff: {0}", mostRecent.Quality);
                    }

                    var rejectionSubject = recent ? "Recent" : "CDH is disabled and";

                    switch (upgradeableRejectReason)
                    {
                        case UpgradeableRejectReason.None:
                            continue;

                        case UpgradeableRejectReason.BetterQuality:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherPreference, "{0} grab event in history is of equal or higher preference: {1}", rejectionSubject, mostRecent.Quality);

                        case UpgradeableRejectReason.BetterRevision:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryHigherRevision, "{0} grab event in history is of equal or higher revision: {1}", rejectionSubject, mostRecent.Quality.Revision);

                        case UpgradeableRejectReason.QualityCutoff:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCutoffMet, "{0} grab event in history meets quality cutoff: {1}", rejectionSubject, qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                        case UpgradeableRejectReason.CustomFormatCutoff:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatCutoffMet, "{0} grab event in history meets Custom Format cutoff: {1}", rejectionSubject, qualityProfile.CutoffFormatScore);

                        case UpgradeableRejectReason.CustomFormatScore:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScore, "{0} grab event in history has an equal or higher Custom Format score: {1}", rejectionSubject, qualityProfile.CalculateCustomFormatScore(customFormats));

                        case UpgradeableRejectReason.MinCustomFormatScore:
                            return DownloadSpecDecision.Reject(DownloadRejectionReason.HistoryCustomFormatScoreIncrement, "{0} grab event in history has Custom Format score within Custom Format score increment: {1}", rejectionSubject, qualityProfile.MinUpgradeFormatScore);
                    }
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
