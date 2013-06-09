using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        EpisodeFile Add(EpisodeFile episodeFile);
        void Update(EpisodeFile episodeFile);
        void Delete(EpisodeFile episodeFile);
        bool Exists(string path);
        EpisodeFile GetFileByPath(string path);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
    }

    public class MediaFileService : IMediaFileService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IConfigService _configService;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;
        private readonly IMediaFileRepository _mediaFileRepository;

        public MediaFileService(IMediaFileRepository mediaFileRepository, IConfigService configService, IMessageAggregator messageAggregator, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _configService = configService;
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

        public void Delete(EpisodeFile episodeFile)
        {
            _mediaFileRepository.Delete(episodeFile);
            _messageAggregator.PublishEvent(new EpisodeFileDeletedEvent(episodeFile));
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

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var files = GetFilesBySeries(message.Series.Id);
            _mediaFileRepository.DeleteMany(files);
        }

        public FileInfo CalculateFilePath(Series series, int seasonNumber, string fileName, string extension)
        {
            string path = series.Path;
            if (series.SeasonFolder)
            {
                var seasonFolder = _configService.SortingSeasonFolderFormat
                                                 .Replace("%0s", seasonNumber.ToString("00"))
                                                 .Replace("%s", seasonNumber.ToString());

                path = Path.Combine(path, seasonFolder);
            }

            path = Path.Combine(path, fileName + extension);

            return new FileInfo(path);
        }
    }
}