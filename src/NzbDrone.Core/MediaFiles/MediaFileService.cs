using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.Core.MediaFiles.Series;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        //Series
        EpisodeFile Add(EpisodeFile episodeFile);
        List<EpisodeFile> GetFilesBySeries(int seriesId);
        List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
        List<MediaModelBase> GetFilesWithoutMediaInfo();
        EpisodeFile GetEpisodeFile(int id);
        List<EpisodeFile> GetEpisodeFiles(IEnumerable<int> ids);

        //Movies
        MovieFile Add(MovieFile movieFile);
        List<MovieFile> GetFileByMovie(int movieId);
        MovieFile GetMovieFile(int movieFileId);
        List<MovieFile> GetMovieFiles(IEnumerable<int> movieFileId);

        //Common
        void Update(MediaModelBase file);
        void Delete(MediaModelBase file, DeleteMediaFileReason reason);
        List<string> FilterExistingFiles(List<string> files, Media media);

    }

    public class MediaFileService : IMediaFileService, IHandleAsync<SeriesDeletedEvent>, IHandleAsync<MovieDeletedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IEpisodeFileRepository _episodeFileRepository;
        private readonly IMovieFileRepository _movieFileRepository;
        private readonly Logger _logger;

        public MediaFileService(IEpisodeFileRepository episodeFileRepository, IMovieFileRepository movieFileRepository, IEventAggregator eventAggregator, Logger logger)
        {
            _episodeFileRepository = episodeFileRepository;
            _movieFileRepository = movieFileRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public EpisodeFile Add(EpisodeFile episodeFile)
        {
            var addedFile = _episodeFileRepository.Insert(episodeFile);
            _eventAggregator.PublishEvent(new EpisodeFileAddedEvent(addedFile));
            return addedFile;
        }

        public void Update(EpisodeFile episodeFile)
        {
            _episodeFileRepository.Update(episodeFile);
        }

        public void Delete(MediaModelBase file, DeleteMediaFileReason reason)
        {
            if (file is EpisodeFile)
            {
                Delete(file as EpisodeFile, reason);
            }
            else if (file is MovieFile)
            {
                Delete(file as MovieFile, reason);
            }
        }

        public void Update(MediaModelBase file)
        {
            if (file is EpisodeFile)
            {
                Update(file as EpisodeFile);
            }
            else if (file is MovieFile)
            {
                Update(file as MovieFile);
            }
        }

        private void Delete(EpisodeFile episodeFile, DeleteMediaFileReason reason)
        {
            //Little hack so we have the episodes and series attached for the event consumers
            episodeFile.Episodes.LazyLoad();
            episodeFile.Path = Path.Combine(episodeFile.Series.Value.Path, episodeFile.RelativePath);

            _episodeFileRepository.Delete(episodeFile);
            _eventAggregator.PublishEvent(new EpisodeFileDeletedEvent(episodeFile, reason));
        }

        public List<EpisodeFile> GetFilesBySeries(int seriesId)
        {
            return _episodeFileRepository.GetFilesBySeries(seriesId);
        }

        public List<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return _episodeFileRepository.GetFilesBySeason(seriesId, seasonNumber);
        }

        public List<MediaModelBase> GetFilesWithoutMediaInfo()
        {
            IEnumerable<MediaModelBase> episodes = _episodeFileRepository.GetFilesWithoutMediaInfo().Select(c => c as MediaModelBase);
            IEnumerable<MediaModelBase> movies = _movieFileRepository.GetFilesWithoutMediaInfo().Select(c => c as MediaModelBase);

            return episodes.Union(movies).ToList();
        }

        public List<string> FilterExistingFiles(List<string> files, Media media)
        {
            if (media is Tv.Series)
            {
                var seriesFiles = GetFilesBySeries(media.Id).Select(f => Path.Combine(media.Path, f.RelativePath)).ToList();

                if (!seriesFiles.Any()) return files;

                return files.Except(seriesFiles, PathEqualityComparer.Instance).ToList();
            }
            else if (media is Movie)
            {
                var movieFile = _movieFileRepository.All().Where(m => m.MovieId == media.Id).SingleOrDefault();

                if (movieFile == null) return files;

                return files.Except(new List<string> { Path.Combine(media.Path, movieFile.RelativePath) }, PathEqualityComparer.Instance).ToList();

            }

            return files;
        }

        public EpisodeFile GetEpisodeFile(int id)
        {
            return _episodeFileRepository.Get(id);
        }

        public List<EpisodeFile> GetEpisodeFiles(IEnumerable<int> ids)
        {
            return _episodeFileRepository.Get(ids).ToList();
        }

        public MovieFile Add(MovieFile movieFile)
        {
            var addedFile = _movieFileRepository.Insert(movieFile);
            _eventAggregator.PublishEvent(new MovieFileAddedEvent(addedFile));
            return addedFile;
        }

        public MovieFile GetMovieFile(int movieFileId)
        {
            return _movieFileRepository.Get(movieFileId);
        }

        public List<MovieFile> GetMovieFiles(IEnumerable<int> movieFileId)
        {
            return _movieFileRepository.Get(movieFileId).ToList();
        }

        public void Update(MovieFile movieFile)
        {
            _movieFileRepository.Update(movieFile);
        }

        private void Delete(MovieFile movieFile, DeleteMediaFileReason reason)
        {
            //Little hack so we have the episodes and series attached for the event consumers
            movieFile.Movie.LazyLoad();
            movieFile.Path = Path.Combine(movieFile.Movie.Value.Path, movieFile.RelativePath);


            _movieFileRepository.Delete(movieFile);
            _eventAggregator.PublishEvent(new MovieFileDeletedEvent(movieFile, reason));
        }

        public List<MovieFile> GetFileByMovie(int movieId)
        {
            return _movieFileRepository.All().Where(m => m.MovieId == movieId).ToList();
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            var files = GetFilesBySeries(message.Series.Id);
            _episodeFileRepository.DeleteMany(files);
        }

        public void HandleAsync(MovieDeletedEvent message)
        {
            var file = GetFileByMovie(message.Movie.Id).SingleOrDefault();
            if (file != null)
                _movieFileRepository.Delete(file);
        }
    }
}