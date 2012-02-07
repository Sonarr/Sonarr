using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class UpgradeHistorySpecification
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly HistoryProvider _historyProvider;
        private readonly QualityUpgradeSpecification _qualityUpgradeSpecification;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public UpgradeHistorySpecification(EpisodeProvider episodeProvider, HistoryProvider historyProvider, QualityUpgradeSpecification qualityUpgradeSpecification)
        {
            _episodeProvider = episodeProvider;
            _historyProvider = historyProvider;
            _qualityUpgradeSpecification = qualityUpgradeSpecification;
        }

        public UpgradeHistorySpecification()
        {

        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            foreach (var episode in _episodeProvider.GetEpisodesByParseResult(subject))
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
