using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        Task DeleteForSeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default);
        Task DeleteForSeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default);
        Task DeleteForEpisodeFileAsync(int episodeFileId, CancellationToken cancellationToken = default);
        Task<List<TExtraFile>> GetFilesBySeriesAsync(int seriesId, CancellationToken cancellationToken = default);
        Task<List<TExtraFile>> GetFilesBySeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default);
        Task<List<TExtraFile>> GetFilesByEpisodeFileAsync(int episodeFileId, CancellationToken cancellationToken = default);
        Task<TExtraFile> FindByPathAsync(int seriesId, string path, CancellationToken cancellationToken = default);
    }

    public class ExtraFileRepository<TExtraFile> : BasicRepository<TExtraFile>, IExtraFileRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task DeleteForSeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(c => seriesIds.Contains(c.SeriesId), cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteForSeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteForEpisodeFileAsync(int episodeFileId, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(c => c.EpisodeFileId == episodeFileId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TExtraFile>> GetFilesBySeriesAsync(int seriesId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.SeriesId == seriesId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TExtraFile>> GetFilesBySeasonAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.SeriesId == seriesId && c.SeasonNumber == seasonNumber, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TExtraFile>> GetFilesByEpisodeFileAsync(int episodeFileId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.EpisodeFileId == episodeFileId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TExtraFile> FindByPathAsync(int seriesId, string path, CancellationToken cancellationToken = default)
        {
            var extraFiles = await QueryAsync(c => c.SeriesId == seriesId && c.RelativePath == path, cancellationToken).ConfigureAwait(false);
            return extraFiles.SingleOrDefault();
        }
    }
}
