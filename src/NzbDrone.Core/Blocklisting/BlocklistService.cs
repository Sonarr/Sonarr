using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
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
                                    IHandleAsync<DownloadFailedEvent>,
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
                    var blocklistedByTorrentInfohash = _blocklistRepository.BlocklistedByTorrentInfoHashAsync(seriesId, torrentInfo.InfoHash).GetAwaiter().GetResult();

                    return blocklistedByTorrentInfohash.Any(b => SameTorrent(b, torrentInfo));
                }

                return _blocklistRepository.BlocklistedByTitleAsync(seriesId, release.Title).GetAwaiter().GetResult()
                    .Where(b => b.Protocol == DownloadProtocol.Torrent)
                    .Any(b => SameTorrent(b, torrentInfo));
            }

            return _blocklistRepository.BlocklistedByTitleAsync(seriesId, release.Title).GetAwaiter().GetResult()
                .Where(b => b.Protocol == DownloadProtocol.Usenet)
                .Any(b => SameNzb(b, release));
        }

        public bool BlocklistedTorrentHash(int seriesId, string hash)
        {
            return _blocklistRepository.BlocklistedByTorrentInfoHashAsync(seriesId, hash).GetAwaiter().GetResult().Any(b =>
                b.TorrentInfoHash.Equals(hash, StringComparison.InvariantCultureIgnoreCase));
        }

        public PagingSpec<Blocklist> Paged(PagingSpec<Blocklist> pagingSpec)
        {
            return _blocklistRepository.GetPagedAsync(pagingSpec).GetAwaiter().GetResult();
        }

        public void Block(RemoteEpisode remoteEpisode, string message, string source)
        {
            var blocklist = new Blocklist
            {
                SeriesId = remoteEpisode.Series.Id,
                EpisodeIds = remoteEpisode.Episodes.Select(e => e.Id).ToList(),
                SourceTitle = remoteEpisode.Release.Title,
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

            _blocklistRepository.InsertAsync(blocklist).GetAwaiter().GetResult();
        }

        public void Delete(int id)
        {
            _blocklistRepository.DeleteAsync(id).GetAwaiter().GetResult();
        }

        public void Delete(List<int> ids)
        {
            _blocklistRepository.DeleteManyAsync(ids).GetAwaiter().GetResult();
        }

        private bool SameNzb(Blocklist item, ReleaseInfo release)
        {
            return ReleaseComparer.SameNzb(new ReleaseComparerModel(item), release);
        }

        private bool SameTorrent(Blocklist item, TorrentInfo release)
        {
            return ReleaseComparer.SameTorrent(new ReleaseComparerModel(item), release);
        }

        public void Execute(ClearBlocklistCommand message)
        {
            _blocklistRepository.PurgeAsync().GetAwaiter().GetResult();
        }

        public async Task HandleAsync(DownloadFailedEvent message, CancellationToken cancellationToken)
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

            await _blocklistRepository.InsertAsync(blocklist, cancellationToken).ConfigureAwait(false);
        }

        public async Task HandleAsync(SeriesDeletedEvent message, CancellationToken cancellationToken)
        {
            await _blocklistRepository.DeleteForSeriesIdsAsync(message.Series.Select(m => m.Id).ToList(), cancellationToken).ConfigureAwait(false);
        }
    }
}
