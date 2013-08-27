using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Common;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        EpisodeFile Add(EpisodeFile episodeFile);
        void Update(EpisodeFile episodeFile);
        void Delete(EpisodeFile episodeFile, bool forUpgrade = false);
        bool Exists(string path);
        EpisodeFile GetFileByPath(string path);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
        List<string> FilterExistingFiles(List<string> files, int seriesId);
        EpisodeFile Get(int id);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IMessageAggregator _messageAggregator;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly Logger _logger;

        public MediaFileService(IMediaFileRepository mediaFileRepository, IMessageAggregator messageAggregator, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        public EpisodeFile Add(EpisodeFile episodeFile)
        {
            var addedFile = _mediaFileRepository.Insert(episodeFile);
            _messageAggregator.PublishEvent(new EpisodeFileAddedEvent(addedFile));
            return addedFile;
        }

        public void Update(EpisodeFile episodeFile)
        {
            _mediaFileRepository.Update(episodeFile);
        }

        public void Delete(EpisodeFile episodeFile, bool forUpgrade = false)
        {
            _mediaFileRepository.Delete(episodeFile);

            _messageAggregator.PublishEvent(new EpisodeFileDeletedEvent(episodeFile, forUpgrade));
        }

        public bool Exists(string path)
        {
            return _mediaFileRepository.Exists(path);
        }

        public EpisodeFile GetFileByPath(string path)
        {
            return _mediaFileRepository.GetFileByPath(path.Normalize());
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

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var files = GetFilesBySeries(message.Series.Id);
            _mediaFileRepository.DeleteMany(files);
        }
    }
}