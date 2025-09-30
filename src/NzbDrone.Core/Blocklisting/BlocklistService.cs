using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Blocklisting
{
    public interface IBlocklistService
    {
        bool Blocklisted(int seriesId, ReleaseInfo release);
        bool BlocklistedTorrentHash(int seriesId, string hash);
        PagingSpec<Blocklist> Paged(PagingSpec<Blocklist> pagingSpec);
        void Block(RemoteEpisode remoteEpisode, string message, string source);
        void Delete(int id);
        void Delete(List<int> ids);
    }

    public class BlocklistService : IBlocklistService,
                                    IExecute<ClearBlocklistCommand>,
                                    IHandle<DownloadFailedEvent>,
                                    IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IBlocklistRepository _blocklistRepository;

        public BlocklistService(IBlocklistRepository blocklistRepository)
        {
            _blocklistRepository = blocklistRepository;
        }

        public bool Blocklisted(int seriesId, ReleaseInfo release)
        {
            if (release.DownloadProtocol == DownloadProtocol.Torrent)
            {
                if (release is not TorrentInfo torrentInfo)
                {
                    return false;
                }

                if (torrentInfo.InfoHash.IsNotNullOrWhiteSpace())
                {
                    var blocklistedByTorrentInfohash = _blocklistRepository.BlocklistedByTorrentInfoHash(seriesId, torrentInfo.InfoHash);

                    return blocklistedByTorrentInfohash.Any(b => SameTorrent(b, torrentInfo));
                }

                return _blocklistRepository.BlocklistedByTitle(seriesId, release.Title)
                    .Where(b => b.Protocol == DownloadProtocol.Torrent)
                    .Any(b => SameTorrent(b, torrentInfo));
            }

            return _blocklistRepository.BlocklistedByTitle(seriesId, release.Title)
                .Where(b => b.Protocol == DownloadProtocol.Usenet)
                .Any(b => SameNzb(b, release));
        }

        public bool BlocklistedTorrentHash(int seriesId, string hash)
        {
            return _blocklistRepository.BlocklistedByTorrentInfoHash(seriesId, hash).Any(b =>
                b.TorrentInfoHash.Equals(hash, StringComparison.InvariantCultureIgnoreCase));
        }

        public PagingSpec<Blocklist> Paged(PagingSpec<Blocklist> pagingSpec)
        {
            return _blocklistRepository.GetPaged(pagingSpec);
        }

        public void Block(RemoteEpisode remoteEpisode, string message, string source)
        {
            var blocklist = new Blocklist
                            {
                                SeriesId = remoteEpisode.Series.Id,
                                EpisodeIds = remoteEpisode.Episodes.Select(e => e.Id).ToList(),
                                SourceTitle =  remoteEpisode.Release.Title,
                                Quality = remoteEpisode.ParsedEpisodeInfo.Quality,
                                Date = DateTime.UtcNow,
                                PublishedDate = remoteEpisode.Release.PublishDate,
                                Size = remoteEpisode.Release.Size,
                                Indexer = remoteEpisode.Release.Indexer,
                                Protocol = remoteEpisode.Release.DownloadProtocol,
                                Message = message,
                                Source = source,
                                Languages = remoteEpisode.ParsedEpisodeInfo.Languages
                            };

            if (remoteEpisode.Release is TorrentInfo torrentRelease)
            {
                blocklist.TorrentInfoHash = torrentRelease.InfoHash;
            }

            _blocklistRepository.Insert(blocklist);
        }

        public void Delete(int id)
        {
            _blocklistRepository.Delete(id);
        }

        public void Delete(List<int> ids)
        {
            _blocklistRepository.DeleteMany(ids);
        }

        private bool SameNzb(Blocklist item, ReleaseInfo release)
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

        private bool SameTorrent(Blocklist item, TorrentInfo release)
        {
            if (release.InfoHash.IsNotNullOrWhiteSpace())
            {
                return release.InfoHash.Equals(item.TorrentInfoHash, StringComparison.InvariantCultureIgnoreCase);
            }

            return HasSameIndexer(item, release.Indexer);
        }

        private bool HasSameIndexer(Blocklist item, string indexer)
        {
            if (item.Indexer.IsNullOrWhiteSpace())
            {
                return true;
            }

            return item.Indexer.Equals(indexer, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HasSamePublishedDate(Blocklist item, DateTime publishedDate)
        {
            if (!item.PublishedDate.HasValue)
            {
                return true;
            }

            return item.PublishedDate.Value.AddMinutes(-2) <= publishedDate &&
                   item.PublishedDate.Value.AddMinutes(2) >= publishedDate;
        }

        private bool HasSameSize(Blocklist item, long size)
        {
            if (!item.Size.HasValue)
            {
                return true;
            }

            var difference = Math.Abs(item.Size.Value - size);

            return difference <= 2.Megabytes();
        }

        public void Execute(ClearBlocklistCommand message)
        {
            _blocklistRepository.Purge();
        }

        public void Handle(DownloadFailedEvent message)
        {
            var blocklist = new Blocklist
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
                Source = message.Source,
                Languages = message.Languages,
                TorrentInfoHash = message.TrackedDownload?.Protocol == DownloadProtocol.Torrent
                    ? message.TrackedDownload.DownloadItem.DownloadId
                    : message.Data.GetValueOrDefault("torrentInfoHash", null)
            };

            if (Enum.TryParse(message.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags))
            {
                blocklist.IndexerFlags = flags;
            }

            if (Enum.TryParse(message.Data.GetValueOrDefault("releaseType"), true, out ReleaseType releaseType))
            {
                blocklist.ReleaseType = releaseType;
            }

            _blocklistRepository.Insert(blocklist);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            _blocklistRepository.DeleteForSeriesIds(message.Series.Select(m => m.Id).ToList());
        }
    }
}
