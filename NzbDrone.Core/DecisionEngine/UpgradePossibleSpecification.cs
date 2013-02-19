using System.Linq;
using NLog;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.DecisionEngine
{
    public class UpgradePossibleSpecification
    {
        private readonly QualityProvider _qualityProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UpgradePossibleSpecification(QualityProvider qualityProvider)
        {
            _qualityProvider = qualityProvider;
        }

        public UpgradePossibleSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(Episode subject)
        {
            //Used to check if the existing episode can be upgraded by searching (Before we search)
            if (subject.EpisodeFileId == 0)
                return true;

            var profile = _qualityProvider.Get(subject.Series.QualityProfileId);


            //TODO:How about proper?
            if (subject.EpisodeFile.Quality >= profile.Cutoff)
                return false;

            return true; ;
        }
    }
}
