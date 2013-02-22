using System.Linq;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.DecisionEngine
{
    public class UpgradeHistorySpecification
    {
        private readonly IEpisodeService _episodeService;
        private readonly HistoryProvider _historyProvider;
        private readonly QualityUpgradeSpecification _qualityUpgradeSpecification;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UpgradeHistorySpecification(IEpisodeService episodeService, HistoryProvider historyProvider, QualityUpgradeSpecification qualityUpgradeSpecification)
        {
            _episodeService = episodeService;
            _historyProvider = historyProvider;
            _qualityUpgradeSpecification = qualityUpgradeSpecification;
        }

        public UpgradeHistorySpecification()
        {

        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            foreach (var episode in subject.Episodes)
            {
                var bestQualityInHistory = _historyProvider.GetBestQualityInHistory(subject.Series.SeriesId, episode.SeasonNumber, episode.EpisodeNumber);
                if (bestQualityInHistory != null)
                {
                    logger.Trace("Comparing history quality with report. History is {0}", bestQualityInHistory);
                    if (!_qualityUpgradeSpecification.IsSatisfiedBy(bestQualityInHistory, subject.Quality, subject.Series.QualityProfile.Cutoff))
                        return false;
                }
            }

            return true;
        }
    }
}
