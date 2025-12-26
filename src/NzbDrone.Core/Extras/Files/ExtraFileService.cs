using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileService<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        List<TExtraFile> GetFilesBySeries(int seriesId);
        List<TExtraFile> GetFilesByEpisodeFile(int episodeFileId);
        TExtraFile FindByPath(int seriesId, string path);
        void Upsert(TExtraFile extraFile);
        void Upsert(List<TExtraFile> extraFiles);
        void Delete(int id);
        void DeleteMany(IEnumerable<int> ids);
    }

    public abstract class ExtraFileService<TExtraFile> : IExtraFileService<TExtraFile>,
                                                         IHandleAsync<SeriesDeletedEvent>,
                                                         IHandleAsync<EpisodeFileDeletedEvent>
        where TExtraFile : ExtraFile, new()
    {
        private readonly IExtraFileRepository<TExtraFile> _repository;
        private readonly ISeriesService _seriesService;
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly Logger _logger;

        public ExtraFileService(IExtraFileRepository<TExtraFile> repository,
                                ISeriesService seriesService,
                                IDiskProvider diskProvider,
                                IRecycleBinProvider recycleBinProvider,
                                Logger logger)
        {
            _repository = repository;
            _seriesService = seriesService;
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _logger = logger;
        }

        public List<TExtraFile> GetFilesBySeries(int seriesId)
        {
            return _repository.GetFilesBySeriesAsync(seriesId).GetAwaiter().GetResult();
        }

        public List<TExtraFile> GetFilesByEpisodeFile(int episodeFileId)
        {
            return _repository.GetFilesByEpisodeFileAsync(episodeFileId).GetAwaiter().GetResult();
        }

        public TExtraFile FindByPath(int seriesId, string path)
        {
            return _repository.FindByPathAsync(seriesId, path).GetAwaiter().GetResult();
        }

        public void Upsert(TExtraFile extraFile)
        {
            Upsert(new List<TExtraFile> { extraFile });
        }

        public void Upsert(List<TExtraFile> extraFiles)
        {
            extraFiles.ForEach(m =>
            {
                m.LastUpdated = DateTime.UtcNow;

                if (m.Id == 0)
                {
                    m.Added = m.LastUpdated;
                }
            });

            _repository.InsertManyAsync(extraFiles.Where(m => m.Id == 0).ToList()).GetAwaiter().GetResult();
            _repository.UpdateManyAsync(extraFiles.Where(m => m.Id > 0).ToList()).GetAwaiter().GetResult();
        }

        public void Delete(int id)
        {
            _repository.DeleteAsync(id).GetAwaiter().GetResult();
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            _repository.DeleteManyAsync(ids).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(SeriesDeletedEvent message, CancellationToken cancellationToken)
        {
            _logger.Debug("Deleting Extra from database for series: {0}", string.Join(',', message.Series));
            await _repository.DeleteForSeriesIdsAsync(message.Series.Select(m => m.Id).ToList(), cancellationToken).ConfigureAwait(false);
        }

        public async Task HandleAsync(EpisodeFileDeletedEvent message, CancellationToken cancellationToken)
        {
            var episodeFile = message.EpisodeFile;

            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing episode file from DB as part of cleanup routine, not deleting extra files from disk.");
            }
            else
            {
                var series = _seriesService.GetSeries(message.EpisodeFile.SeriesId);

                // TODO: Add async disk provider method
                foreach (var extra in await _repository.GetFilesByEpisodeFileAsync(episodeFile.Id, cancellationToken))
                {
                    var path = Path.Combine(series.Path, extra.RelativePath);

                    if (_diskProvider.FileExists(path))
                    {
                        // Send to the recycling bin so they can be recovered if necessary
                        var subfolder = _diskProvider.GetParentFolder(series.Path).GetRelativePath(_diskProvider.GetParentFolder(path));
                        _recycleBinProvider.DeleteFile(path, subfolder);
                    }
                }
            }

            _logger.Debug("Deleting Extra from database for episode file: {0}", episodeFile);

            await _repository.DeleteForEpisodeFileAsync(episodeFile.Id, cancellationToken).ConfigureAwait(false);
        }
    }
}
