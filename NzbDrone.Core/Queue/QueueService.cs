using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Queue
{
    public interface IQueueService
    {
        List<Queue> GetQueue();
    }

    public class QueueService : IQueueService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public QueueService(IProvideDownloadClient downloadClientProvider, IParsingService parsingService, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _parsingService = parsingService;
            _logger = logger;
        }

        public List<Queue> GetQueue()
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();
            var queueItems = downloadClient.GetQueue();

            return MapQueue(queueItems);
        }

        private List<Queue> MapQueue(IEnumerable<QueueItem> queueItems)
        {
            var queued = new List<Queue>();

            foreach (var queueItem in queueItems)
            {
                var parsedEpisodeInfo = Parser.Parser.ParseTitle(queueItem.Title);

                if (parsedEpisodeInfo != null && !string.IsNullOrWhiteSpace(parsedEpisodeInfo.SeriesTitle))
                {
                    var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);

                    if (remoteEpisode.Series == null)
                    {
                        continue;
                    }

                    foreach (var episode in remoteEpisode.Episodes)
                    {
                        var queue = new Queue();
                        queue.Id = queueItem.Id.GetHashCode();
                        queue.Series = remoteEpisode.Series;
                        queue.Episode = episode;
                        queue.Quality = remoteEpisode.ParsedEpisodeInfo.Quality;
                        queue.Title = queueItem.Title;
                        queue.Size = queueItem.Size;
                        queue.Sizeleft = queueItem.SizeLeft;
                        queue.Timeleft = queueItem.Timeleft;
                        queued.Add(queue);
                    }
                }
            }

            return queued;
        }
    }
}
