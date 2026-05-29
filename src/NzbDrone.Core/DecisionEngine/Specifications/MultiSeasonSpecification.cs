using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MultiSeasonSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MultiSeasonSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (!subject.ParsedEpisodeInfo.IsMultiSeason)
            {
                return DownloadSpecDecision.Accept();
            }

            if (!_configService.EnableExperimentalMultiSeasonSupport)
            {
                _logger.Debug("Multi-season support is disabled. Rejecting: {0}", subject.Release.Title);
                return DownloadSpecDecision.Reject(
                    DownloadRejectionReason.MultiSeason,
                    "Multi-season releases are not supported. Enable experimental multi-season support in Media Management settings to allow them.");
            }

            _logger.Debug("Checking multi-season release: {0}", subject.Release.Title);

            if (!subject.Series.Monitored)
            {
                _logger.Debug("Multi-season release rejected. Series '{0}' is not monitored.", subject.Series.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.SeriesNotMonitored, "Series is not monitored");
            }

            var seasonNumberSource = subject.MappedSeasonNumbers.Any()
                ? subject.MappedSeasonNumbers
                : subject.ParsedEpisodeInfo.SeasonNumbers;

            var coveredSeasonNumbers = seasonNumberSource
                .Where(n => n > 0)
                .Distinct()
                .ToList();

            if (!coveredSeasonNumbers.Any())
            {
                _logger.Debug("Multi-season release rejected. No season numbers parsed.");
                return DownloadSpecDecision.Reject(
                    DownloadRejectionReason.MultiSeasonNotAllMonitored,
                    "Multi-season release rejected. Unable to determine covered seasons.");
            }

            foreach (var seasonNumber in coveredSeasonNumbers)
            {
                var season = subject.Series.Seasons.FirstOrDefault(s => s.SeasonNumber == seasonNumber);

                if (season == null || !season.Monitored)
                {
                    _logger.Debug("Multi-season release rejected. Season {0} is not monitored.", seasonNumber);
                    return DownloadSpecDecision.Reject(
                        DownloadRejectionReason.MultiSeasonNotAllMonitored,
                        $"Multi-season release rejected. Season {seasonNumber} is not monitored.");
                }
            }

            if (!subject.Episodes.Any())
            {
                _logger.Debug("Multi-season release rejected. No episodes resolved.");
                return DownloadSpecDecision.Reject(
                    DownloadRejectionReason.MultiSeasonNotAllMonitored,
                    "Multi-season release rejected. No episodes could be resolved for the covered seasons.");
            }

            var unmonitoredEpisodes = subject.Episodes.Where(e => !e.Monitored).ToList();
            if (unmonitoredEpisodes.Any())
            {
                _logger.Debug("Multi-season release rejected. {0} episode(s) are not monitored.", unmonitoredEpisodes.Count);
                return DownloadSpecDecision.Reject(
                    DownloadRejectionReason.EpisodeNotMonitored,
                    "Multi-season release rejected. One or more episodes are not monitored.");
            }

            if (subject.Episodes.Any(e => !e.AirDateUtc.HasValue || e.AirDateUtc.Value.After(DateTime.UtcNow.AddHours(24))))
            {
                _logger.Debug("Multi-season release {0} rejected. Not all episodes have aired yet.", subject.Release.Title);
                return DownloadSpecDecision.Reject(
                    DownloadRejectionReason.FullSeasonNotAired,
                    "Multi-season release rejected. Not all episodes have aired yet.");
            }

            _logger.Debug("Multi-season release {0} accepted.", subject.Release.Title);
            return DownloadSpecDecision.Accept();
        }
    }
}
