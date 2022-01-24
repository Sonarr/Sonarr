using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDecisionEngineSpecification
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

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return Decision.Accept();
            }

            var cdhEnabled = _configService.EnableCompletedDownloadHandling;

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

                    var customFormats = _formatService.ParseCustomFormat(mostRecent);

                    // The series will be the same as the one in history since it's the same episode.
                    // Instead of fetching the series from the DB reuse the known series.
                    var cutoffUnmet = _upgradableSpecification.CutoffNotMet(
                        subject.Series.QualityProfile,
                        subject.Series.LanguageProfile,
                        mostRecent.Quality,
                        mostRecent.Language,
                        customFormats,
                        subject.ParsedEpisodeInfo.Quality);

                    var upgradeable = _upgradableSpecification.IsUpgradable(
                        subject.Series.QualityProfile,
                        subject.Series.LanguageProfile,
                        mostRecent.Quality,
                        mostRecent.Language,
                        customFormats,
                        subject.ParsedEpisodeInfo.Quality,
                        subject.ParsedEpisodeInfo.Language,
                        subject.CustomFormats);

                    if (!cutoffUnmet)
                    {
                        if (recent)
                        {
                            return Decision.Reject("Recent grab event in history already meets cutoff: {0}", mostRecent.Quality);
                        }

                        return Decision.Reject("CDH is disabled and grab event in history already meets cutoff: {0}", mostRecent.Quality);
                    }

                    if (!upgradeable)
                    {
                        if (recent)
                        {
                            return Decision.Reject("Recent grab event in history is of equal or higher quality: {0}", mostRecent.Quality);
                        }

                        return Decision.Reject("CDH is disabled and grab event in history is of equal or higher quality: {0}", mostRecent.Quality);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
