using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public interface IAggregateRemoteEpisode
    {
        RemoteEpisode Aggregate(RemoteEpisode remoteEpisode);
    }
}
