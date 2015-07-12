using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistService
    {
        bool Blacklisted(int seriesId, ReleaseInfo release);
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

        public bool Blacklisted(int seriesId, ReleaseInfo release)
        {
            var blacklistedByTitle = _blacklistRepository.BlacklistedByTitle(seriesId, release.Title);
            
            if (release.DownloadProtocol == DownloadProtocol.Torrent)
            {
                var torrentInfo = release as TorrentInfo;

                if (torrentInfo == null) return false;

                if (torrentInfo.InfoHash.IsNullOrWhiteSpace())
                {
                    return blacklistedByTitle.Where(b => b.Protocol == DownloadProtocol.Torrent)
                                             .Any(b => SameTorrent(b, torrentInfo));
                }

                var blacklistedByTorrentInfohash = _blacklistRepository.BlacklistedByTorrentInfoHash(seriesId, torrentInfo.InfoHash);

                return blacklistedByTorrentInfohash.Any(b => SameTorrent(b, torrentInfo));
            }

            return blacklistedByTitle.Where(b => b.Protocol == DownloadProtocol.Usenet)
                                     .Any(b => SameNzb(b, release));
        }

        public PagingSpec<Blacklist> Paged(PagingSpec<Blacklist> pagingSpec)
        {
            return _blacklistRepository.GetPaged(pagingSpec);
        }

        public void Delete(int id)
        {
            _blacklistRepository.Delete(id);
        }

        private bool SameNzb(Blacklist item, ReleaseInfo release)
        {
            if (item.PublishedDate == release.PublishDate)
            {
                return true;
            }

            if (!HasSameIndexer(item, release.Indexer) &&
                HasSamePublishedDate(item, release.PublishDate) &&
                HasSameSize(item, release.Size))
            {
                return true;
            }

            return false;
        }

        private bool SameTorrent(Blacklist item, TorrentInfo release)
        {
            if (release.InfoHash.IsNotNullOrWhiteSpace())
            {
                return release.InfoHash.Equals(item.TorrentInfoHash);
            }

            return item.Indexer.Equals(release.Indexer, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HasSameIndexer(Blacklist item, string indexer)
        {
            if (item.Indexer.IsNullOrWhiteSpace())
            {
                return true;
            }

            return item.Indexer.Equals(indexer, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HasSamePublishedDate(Blacklist item, DateTime publishedDate)
        {
            if (!item.PublishedDate.HasValue) return true;

            return item.PublishedDate.Value.AddMinutes(-2) <= publishedDate &&
                   item.PublishedDate.Value.AddMinutes(2) >= publishedDate;
        }

        private bool HasSameSize(Blacklist item, long size)
        {
            if (!item.Size.HasValue) return true;

            var difference = Math.Abs(item.Size.Value - size);

            return difference <= 2.Megabytes();
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
                                PublishedDate = DateTime.Parse(message.Data.GetValueOrDefault("publishedDate")),
                                Size = long.Parse(message.Data.GetValueOrDefault("size", "0")),
                                Indexer = message.Data.GetValueOrDefault("indexer"),
                                Protocol = (DownloadProtocol)Convert.ToInt32(message.Data.GetValueOrDefault("protocol")),
                                Message = message.Message,
                                TorrentInfoHash = message.Data.GetValueOrDefault("torrentInfoHash"),
                                Language = message.Language
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
