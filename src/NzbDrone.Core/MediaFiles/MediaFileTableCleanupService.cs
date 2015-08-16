using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(Media media, List<string> filesOnDisk);
    }

    public class MediaFileTableCleanupService : IMediaFileTableCleanupService
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IEpisodeService _episodeService;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            IEpisodeService episodeService,
                                            IMovieService movieService,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _episodeService = episodeService;
            _movieService = movieService;
            _logger = logger;
        }

        public void Clean(Media media, List<string> filesOnDisk)
        {
            if (media is Tv.Series)
            {
                Clean(media as Tv.Series, filesOnDisk);
            }
            else if (media is Movie)
            {
                Clean(media as Movie, filesOnDisk);
            }
        }

        private void Clean(Tv.Series series, List<string> filesOnDisk)
        {
            var seriesFiles = _mediaFileService.GetFilesBySeries(series.Id);
            var episodes = _episodeService.GetEpisodeBySeries(series.Id);

            var filesOnDiskKeys = new HashSet<String>(filesOnDisk, PathEqualityComparer.Instance);
            
            foreach (var seriesFile in seriesFiles)
            {
                var episodeFile = seriesFile;
                var episodeFilePath = Path.Combine(series.Path, episodeFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(episodeFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", episodeFilePath);
                        _mediaFileService.Delete(seriesFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }

                    if (episodes.None(e => e.EpisodeFileId == episodeFile.Id))
                    {
                        _logger.Debug("File [{0}] is not assigned to any episodes, removing from db", episodeFilePath);
                        _mediaFileService.Delete(episodeFile, DeleteMediaFileReason.NoLinkedEpisodes);
                        continue;
                    }

//                    var localEpsiode = _parsingService.GetLocalEpisode(episodeFile.Path, series);
//
//                    if (localEpsiode == null || episodes.Count != localEpsiode.Episodes.Count)
//                    {
//                        _logger.Debug("File [{0}] parsed episodes has changed, removing from db", episodeFile.Path);
//                        _mediaFileService.Delete(episodeFile);
//                        continue;
//                    }
                }

                catch (Exception ex)
                {
                    var errorMessage = String.Format("Unable to cleanup EpisodeFile in DB: {0}", episodeFile.Id);
                    _logger.ErrorException(errorMessage, ex);
                }
            }

            foreach (var e in episodes)
            {
                var episode = e;

                if (episode.EpisodeFileId > 0 && seriesFiles.None(f => f.Id == episode.EpisodeFileId))
                {
                    episode.EpisodeFileId = 0;
                    _episodeService.UpdateEpisode(episode);
                }
            }
        }

        private void Clean(Movie movie, List<string> filesOnDisk)
        {
            var movieFiles = _mediaFileService.GetFileByMovie(movie.Id);
            
            var filesOnDiskKeys = new HashSet<String>(filesOnDisk, PathEqualityComparer.Instance);

            foreach (var movieFile in movieFiles)
            {
                var theFile = movieFile;
                var theFilePath = Path.Combine(movie.Path, theFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(theFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", theFilePath);
                        _mediaFileService.Delete(movieFile, DeleteMediaFileReason.MissingFromDisk);
                    }
                }

                catch (Exception ex)
                {
                    var errorMessage = String.Format("Unable to cleanup EpisodeFile in DB: {0}", theFile.Id);
                    _logger.ErrorException(errorMessage, ex);
                }

                if (movie.MovieFileId > 0 && movieFile.Id != movie.MovieFileId)
                {
                    movie.MovieFileId = 0;
                    _movieService.UpdateMovie(movie);
                }
            }
        }
    }
}