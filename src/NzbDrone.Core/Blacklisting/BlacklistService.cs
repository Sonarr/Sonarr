using System;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistService
    {
        bool Blacklisted(string sourceTitle);
    }

    public class BlacklistService : IBlacklistService, IHandle<DownloadFailedEvent>
    {
        private readonly IBlacklistRepository _blacklistRepository;
        private readonly IRedownloadFailedDownloads _redownloadFailedDownloadService;

        public BlacklistService(IBlacklistRepository blacklistRepository, IRedownloadFailedDownloads redownloadFailedDownloadService)
        {
            _blacklistRepository = blacklistRepository;
            _redownloadFailedDownloadService = redownloadFailedDownloadService;
        }

        public bool Blacklisted(string sourceTitle)
        {
            return _blacklistRepository.Blacklisted(sourceTitle);
        }

        public void Handle(DownloadFailedEvent message)
        {
            var blacklist = new Blacklist
                            {
                                SeriesId = message.SeriesId,
                                EpisodeIds = message.EpisodeIds,
                                SourceTitle = message.SourceTitle,
                                Quality = message.Quality,
                                Date = DateTime.UtcNow
                            };

            _blacklistRepository.Insert(blacklist);

            _redownloadFailedDownloadService.Redownload(message.SeriesId, message.EpisodeIds);
        }
    }
}
