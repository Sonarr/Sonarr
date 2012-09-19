using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class EpisodeAiredAfterCutoffSpecification
    {
        private readonly EpisodeProvider _episodeProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public EpisodeAiredAfterCutoffSpecification(EpisodeProvider episodeProvider)
        {
            _episodeProvider = episodeProvider;
        }

        public EpisodeAiredAfterCutoffSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            if (!subject.Series.DownloadEpisodesAiredAfter.HasValue)
            {
                logger.Debug("{0} does not restrict downloads before date.", subject.Series.Title);
                return true;
            }

            var episodes = _episodeProvider.GetEpisodesByParseResult(subject);

            if (episodes.Any(episode => episode.AirDate > subject.Series.DownloadEpisodesAiredAfter.Value))
            {
                logger.Debug("One or more episodes aired after cutoff, downloading.");
                return true;
            }

            logger.Debug("Episodes aired before cutoff date: {0}", subject.Series.DownloadEpisodesAiredAfter);
            return false;
        }
    }
}
