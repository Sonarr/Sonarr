using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv.Commands;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class EpisodeTitleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public EpisodeTitleSpecification(IManageCommandQueue commandQueueManager, Logger logger)
        {
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }
        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            foreach (var episode in localEpisode.Episodes)
            {
                var seriesId = localEpisode.Series.Id;
                var airDateUtc = episode.AirDateUtc;
                var title = episode.Title;
                var episodeNumber = episode.EpisodeNumber;

                if (airDateUtc.HasValue && airDateUtc.Value.Before(DateTime.UtcNow.AddDays(-1)))
                {
                    _logger.Debug("Episode aired more than 1 day ago");
                    continue;
                }

                if (title.IsNullOrWhiteSpace())
                {
                    _logger.Debug("Episode does not have a title and recently aired");
                    _commandQueueManager.Push(new RefreshSeriesCommand(seriesId));

                    return Decision.Reject("Episode does not have a title and recently aired");
                }

                if (title.Equals("TBA"))
                {
                    _logger.Debug("Episode has a TBA title and recently aired");
                    _commandQueueManager.Push(new RefreshSeriesCommand(seriesId));

                    return Decision.Reject("Episode has a TBA title and recently aired");
                }

                if (title.Equals($"Episode {episodeNumber}", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Episode has a placeholder title and recently aired");
                    _commandQueueManager.Push(new RefreshSeriesCommand(seriesId));

                    return Decision.Reject("Episode has a placeholder a title and recently aired");
                }

                if (title.Equals($"Episode {episodeNumber:00}", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Episode has a placeholder title and recently aired");
                    _commandQueueManager.Push(new RefreshSeriesCommand(seriesId));

                    return Decision.Reject("Episode has a placeholder a title and recently aired");
                }
            }

            return Decision.Accept();
        }
    }
}
