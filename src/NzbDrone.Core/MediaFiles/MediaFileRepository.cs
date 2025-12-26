using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<EpisodeFile>
    {
        Task<List<EpisodeFile>> GetFilesBySeriesAsync(int seriesId, CancellationToken cancellationToken = default);
        Task<List<EpisodeFile>> GetFilesBySeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default);
        Task<List<EpisodeFile>> GetFilesBySeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default);
        Task<List<EpisodeFile>> GetFilesWithoutMediaInfoAsync(CancellationToken cancellationToken = default);
        Task<List<EpisodeFile>> GetFilesWithRelativePathAsync(int seriesId, string relativePath, CancellationToken cancellationToken = default);
        Task DeleteForSeriesAsync(List<int> seriesIds, CancellationToken cancellationToken = default);
    }

    public class MediaFileRepository : BasicRepository<EpisodeFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task<List<EpisodeFile>> GetFilesBySeriesAsync(int seriesId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.SeriesId == seriesId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<EpisodeFile>> GetFilesBySeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => seriesIds.Contains(c.SeriesId), cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<EpisodeFile>> GetFilesBySeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<EpisodeFile>> GetFilesWithoutMediaInfoAsync(CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.MediaInfo == null, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<EpisodeFile>> GetFilesWithRelativePathAsync(int seriesId, string relativePath, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.SeriesId == seriesId && c.RelativePath == relativePath, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteForSeriesAsync(List<int> seriesIds, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(x => seriesIds.Contains(x.SeriesId), cancellationToken).ConfigureAwait(false);
        }
    }
}
