using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public class AggregatePreferredWordScore : IAggregateRemoteEpisode
    {
        private readonly IPreferredWordService _preferredWordServiceCalculator;

        public AggregatePreferredWordScore(IPreferredWordService preferredWordServiceCalculator)
        {
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
        }

        public RemoteEpisode Aggregate(RemoteEpisode remoteEpisode)
        {
            remoteEpisode.PreferredWordScore = _preferredWordServiceCalculator.Calculate(remoteEpisode.Series, remoteEpisode.Release.Title, remoteEpisode.Release.IndexerId);

            return remoteEpisode;
        }
    }
}
