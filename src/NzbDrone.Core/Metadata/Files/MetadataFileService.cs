using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Metadata.Files
{
    public interface IMetadataFileService
    {
        List<MetadataFile> GetFilesBySeries(int seriesId);
        List<MetadataFile> GetFilesByEpisodeFile(int episodeFileId);
        MetadataFile FindByPath(string path);
        List<string> FilterExistingFiles(List<string> files, Series series);
        MetadataFile Upsert(MetadataFile metadataFile);
        void Delete(int id);
    }

    public class MetadataFileService : IMetadataFileService,
                                              IHandleAsync<SeriesDeletedEvent>,
                                              IHandleAsync<EpisodeFileDeletedEvent>,
                                              IHandle<MetadataFileUpdated>
    {
        private readonly IMetadataFileRepository _repository;
        private readonly ISeriesService _seriesService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public MetadataFileService(IMetadataFileRepository repository,
                                          ISeriesService seriesService,
                                          IDiskProvider diskProvider,
                                          Logger logger)
        {
            _repository = repository;
            _seriesService = seriesService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<MetadataFile> GetFilesBySeries(int seriesId)
        {
            return _repository.GetFilesBySeries(seriesId);
        }

        public List<MetadataFile> GetFilesByEpisodeFile(int episodeFileId)
        {
            return _repository.GetFilesByEpisodeFile(episodeFileId);
        }

        public MetadataFile FindByPath(string path)
        {
            return _repository.FindByPath(path);
        }

        public List<string> FilterExistingFiles(List<string> files, Series series)
        {
            var seriesFiles = GetFilesBySeries(series.Id).Select(f => Path.Combine(series.Path, f.RelativePath)).ToList();

            if (!seriesFiles.Any()) return files;

            return files.Except(seriesFiles, PathEqualityComparer.Instance).ToList();
        }

        public MetadataFile Upsert(MetadataFile metadataFile)
        {
            metadataFile.LastUpdated = DateTime.UtcNow;
            return _repository.Upsert(metadataFile);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            _logger.Trace("Deleting Metadata from database for series: {0}", message.Series);
            _repository.DeleteForSeries(message.Series.Id);
        }

        public void HandleAsync(EpisodeFileDeletedEvent message)
        {
            var episodeFile = message.EpisodeFile;
            var series = _seriesService.GetSeries(message.EpisodeFile.SeriesId);

            foreach (var metadata in _repository.GetFilesByEpisodeFile(episodeFile.Id))
            {
                var path = Path.Combine(series.Path, metadata.RelativePath);

                if (_diskProvider.FileExists(path))
                {
                    _diskProvider.DeleteFile(path);
                }
            }

            _logger.Trace("Deleting Metadata from database for episode file: {0}", episodeFile);
            _repository.DeleteForEpisodeFile(episodeFile.Id);
        }

        public void Handle(MetadataFileUpdated message)
        {
            Upsert(message.Metadata);
        }
    }
}
