using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistService
    {
        bool Blacklisted(int seriesId, string sourceTitle, DateTime publishedDate);
        PagingSpec<Blacklist> Paged(PagingSpec<Blacklist> pagingSpec);
        void Delete(int id);
    }

    public class BlacklistService : IBlacklistService,
                                    IExecute<ClearBlacklistCommand>,
                                    IHandle<DownloadFailedEvent>,
                                    IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IBlacklistRepository _blacklistRepository;

        public BlacklistService(IBlacklistRepository blacklistRepository)
        {
            _blacklistRepository = blacklistRepository;
        }

        public bool Blacklisted(int seriesId, string sourceTitle, DateTime publishedDate)
        {
            var blacklisted = _blacklistRepository.Blacklisted(seriesId, sourceTitle);

            return blacklisted.Any(item => HasSamePublishedDate(item, publishedDate));
        }

        public PagingSpec<Blacklist> Paged(PagingSpec<Blacklist> pagingSpec)
        {
            return _blacklistRepository.GetPaged(pagingSpec);
        }

        public void Delete(int id)
        {
            _blacklistRepository.Delete(id);
        }

        private static bool HasSamePublishedDate(Blacklist item, DateTime publishedDate)
        {
            if (!item.PublishedDate.HasValue) return true;

            return item.PublishedDate.Value.AddDays(-2) <= publishedDate &&
                   item.PublishedDate.Value.AddDays(2) >= publishedDate;
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
                                Date = DateTime.UtcNow,
                                PublishedDate = DateTime.Parse(message.Data.GetValueOrDefault("publishedDate"))
                            };

            _blacklistRepository.Insert(blacklist);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var blacklisted = _blacklistRepository.BlacklistedBySeries(message.Series.Id);

            _blacklistRepository.DeleteMany(blacklisted);
        }
    }
}
