using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Download.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation
{
    public interface IRemoteEpisodeAggregationService
    {
        RemoteEpisode Augment(RemoteEpisode remoteEpisode);
    }

    public class RemoteEpisodeAggregationService : IRemoteEpisodeAggregationService
    {
        private readonly IEnumerable<IAggregateRemoteEpisode> _augmenters;
        private readonly Logger _logger;

        public RemoteEpisodeAggregationService(IEnumerable<IAggregateRemoteEpisode> augmenters,
                                  Logger logger)
        {
            _augmenters = augmenters;
            _logger = logger;
        }

        public RemoteEpisode Augment(RemoteEpisode remoteEpisode)
        {
            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(remoteEpisode);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return remoteEpisode;
        }
    }
}
