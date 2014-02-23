using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistService
    {
        bool Blacklisted(string sourceTitle);
        PagingSpec<Blacklist> Paged(PagingSpec<Blacklist> pagingSpec);
        void Delete(int id);
    }

    public class BlacklistService : IBlacklistService, IExecute<ClearBlacklistCommand>, IHandle<DownloadFailedEvent>, IHandle<SeriesDeletedEvent>
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

        public PagingSpec<Blacklist> Paged(PagingSpec<Blacklist> pagingSpec)
        {
            return _blacklistRepository.GetPaged(pagingSpec);
        }

        public void Delete(int id)
        {
            _blacklistRepository.Delete(id);
        }

        public void Execute(ClearBlacklistCommand message)
        {
            _blacklistRepository.Purge();
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

        public void Handle(SeriesDeletedEvent message)
        {
            var blacklisted = _blacklistRepository.BlacklistedBySeries(message.Series.Id);

            _blacklistRepository.DeleteMany(blacklisted);
        }
    }
}
