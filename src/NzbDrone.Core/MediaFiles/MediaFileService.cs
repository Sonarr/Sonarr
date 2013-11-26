using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Common;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        EpisodeFile Add(EpisodeFile episodeFile);
        void Update(EpisodeFile episodeFile);
        void Delete(EpisodeFile episodeFile, bool forUpgrade = false);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
        List<string> FilterExistingFiles(List<string> files, int seriesId);
        EpisodeFile Get(int id);
        List<EpisodeFile> Get(IEnumerable<int> ids);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly Logger _logger;

        public MediaFileService(IMediaFileRepository mediaFileRepository, IEventAggregator eventAggregator, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public EpisodeFile Add(EpisodeFile episodeFile)
        {
            var addedFile = _mediaFileRepository.Insert(episodeFile);
            _eventAggregator.PublishEvent(new EpisodeFileAddedEvent(addedFile));
            return addedFile;
        }

        public void Update(EpisodeFile episodeFile)
        {
            _mediaFileRepository.Update(episodeFile);
        }

        public void Delete(EpisodeFile episodeFile, bool forUpgrade = false)
        {
            _mediaFileRepository.Delete(episodeFile);

            _eventAggregator.PublishEvent(new EpisodeFileDeletedEvent(episodeFile, forUpgrade));
        }

        public List<EpisodeFile> GetFilesBySeries(int seriesId)
        {
            return _mediaFileRepository.GetFilesBySeries(seriesId);
        }

        public List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return _mediaFileRepository.GetFilesBySeason(seriesId, seasonNumber);
        }

        public List<string> FilterExistingFiles(List<string> files, int seriesId)
        {
            var seriesFiles = GetFilesBySeries(seriesId).Select(f => f.Path.CleanFilePath()).ToList();

            if (!seriesFiles.Any()) return files;

            return files.Select(f => f.CleanFilePath()).Except(seriesFiles, new PathEqualityComparer()).ToList();
        }

        public EpisodeFile Get(int id)
        {
            return _mediaFileRepository.Get(id);
        }

        public List<EpisodeFile> Get(IEnumerable<int> ids)
        {
            return _mediaFileRepository.Get(ids).ToList();
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var files = GetFilesBySeries(message.Series.Id);
            _mediaFileRepository.DeleteMany(files);
        }
    }
}