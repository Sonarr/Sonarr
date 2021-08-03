using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeAddedService
    {
        void SearchForRecentlyAdded(int seriesId);
    }

    public class EpisodeAddedService : IHandle<EpisodeInfoRefreshedEvent>, IEpisodeAddedService
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;
        private readonly ICached<List<int>> _addedEpisodesCache;

        public EpisodeAddedService(ICacheManager cacheManager,
                                   IManageCommandQueue commandQueueManager,
                                   IEpisodeService episodeService,
                                   Logger logger)
        {
            _commandQueueManager = commandQueueManager;
            _episodeService = episodeService;
            _logger = logger;
            _addedEpisodesCache = cacheManager.GetCache<List<int>>(GetType());
        }

        public void SearchForRecentlyAdded(int seriesId)
        {
            var previouslyAired = _addedEpisodesCache.Find(seriesId.ToString());

            if (previouslyAired != null && previouslyAired.Any())
            {
                var missing = previouslyAired.Select(e => _episodeService.GetEpisode(e)).Where(e => !e.HasFile).ToList();

                if (missing.Any())
                {
                    _commandQueueManager.Push(new EpisodeSearchCommand(missing.Select(e => e.Id).ToList()));
                }
            }

            _addedEpisodesCache.Remove(seriesId.ToString());
        }

        public void Handle(EpisodeInfoRefreshedEvent message)
        {
            if (message.Series.AddOptions == null)
            {
                if (!message.Series.Monitored)
                {
                    _logger.Debug("Series is not monitored");
                    return;
                }

                if (message.Added.Empty())
                {
                    _logger.Debug("No new episodes, skipping search");
                    return;
                }

                if (message.Added.None(a => a.AirDateUtc.HasValue))
                {
                    _logger.Debug("No new episodes have an air date");
                    return;
                }

                var previouslyAired = message.Added.Where(a => a.AirDateUtc.HasValue
                    && a.AirDateUtc.Value.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(1))
                    && a.Monitored).ToList();

                if (previouslyAired.Empty())
                {
                    _logger.Debug("Newly added episodes all air in the future");
                    return;
                }

                _addedEpisodesCache.Set(message.Series.Id.ToString(), previouslyAired.Select(e => e.Id).ToList());
            }
        }
    }
}
