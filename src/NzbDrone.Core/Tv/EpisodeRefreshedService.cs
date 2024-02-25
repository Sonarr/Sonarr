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
    public interface IEpisodeRefreshedService
    {
        void Search(int seriesId);
    }

    public class EpisodeRefreshedService : IEpisodeRefreshedService, IHandle<EpisodeInfoRefreshedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;
        private readonly ICached<List<int>> _searchCache;

        public EpisodeRefreshedService(ICacheManager cacheManager,
                                   IManageCommandQueue commandQueueManager,
                                   IEpisodeService episodeService,
                                   Logger logger)
        {
            _commandQueueManager = commandQueueManager;
            _episodeService = episodeService;
            _logger = logger;
            _searchCache = cacheManager.GetCache<List<int>>(GetType());
        }

        public void Search(int seriesId)
        {
            var previouslyAired = _searchCache.Find(seriesId.ToString());

            if (previouslyAired != null && previouslyAired.Any())
            {
                var missing = previouslyAired.Select(e => _episodeService.GetEpisode(e)).Where(e => !e.HasFile).ToList();

                if (missing.Any())
                {
                    _commandQueueManager.Push(new EpisodeSearchCommand(missing.Select(e => e.Id).ToList()));
                }
            }

            _searchCache.Remove(seriesId.ToString());
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

                var previouslyAired = message.Added.Where(a =>
                        a.AirDateUtc.HasValue &&
                        a.AirDateUtc.Value.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(1)) &&
                        a.Monitored)
                    .ToList();

                if (previouslyAired.Empty())
                {
                    _logger.Debug("Newly added episodes all air in the future");
                    _searchCache.Set(message.Series.Id.ToString(), previouslyAired.Select(e => e.Id).ToList());
                }

                var absoluteEpisodeNumberAdded = message.Updated.Where(a =>
                        a.AbsoluteEpisodeNumberAdded &&
                        a.AirDateUtc.HasValue &&
                        a.AirDateUtc.Value.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(1)) &&
                        a.Monitored)
                    .ToList();

                if (absoluteEpisodeNumberAdded.Empty())
                {
                    _logger.Debug("No updated episodes recently aired and had absolute episode number added");
                }

                var toSearch = new List<int>();

                toSearch.AddRange(previouslyAired.Select(e => e.Id));
                toSearch.AddRange(absoluteEpisodeNumberAdded.Select(e => e.Id));

                if (toSearch.Any())
                {
                    _searchCache.Set(message.Series.Id.ToString(), toSearch.Distinct().ToList());
                }
            }
        }
    }
}
