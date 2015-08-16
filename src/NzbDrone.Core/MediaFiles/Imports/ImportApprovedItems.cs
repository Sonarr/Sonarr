using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.Core.MediaFiles.Series;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;


namespace NzbDrone.Core.MediaFiles.Imports
{
    public interface IImportApprovedItems
    {
        List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null);
    }

    public class ImportApprovedItems : IImportApprovedItems
    {
        private readonly IUpgradeMediaFiles _mediaFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportApprovedItems(IUpgradeMediaFiles mediaFileUpgrader,
                                   IMediaFileService mediaFileService,
                                   IDiskProvider diskProvider,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _mediaFileUpgrader = mediaFileUpgrader;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private IEnumerable<ImportDecision> OrderDecision(List<ImportDecision> decisions)
        {
            if (!decisions.Any())
                return decisions;

            return decisions.OrderByDescending(e =>
            {
                var localEpisode = e.LocalItem as LocalEpisode;
                if (localEpisode != null)
                {
                    return localEpisode.Episodes.Select(episode => episode.EpisodeNumber).MinOrDefault();
                }
                return 0;
            })
            .ThenByDescending(e => e.LocalItem.Size);
        }

        private List<ImportResult> ImportDecision(List<ImportResult> results, ImportDecision decision, bool newDownload, DownloadClientItem downloadClientItem = null)
        {
            if (decision.LocalItem is LocalEpisode)
            {
                return ImportEpisodeDecision(results, decision, newDownload, downloadClientItem);
            }
            else if (decision.LocalItem is LocalMovie)
            {
                return ImportMovieDecision(results, decision, newDownload, downloadClientItem);
            }

            results.Add(new ImportResult(decision, "Unknown item type"));
            return results;
        }

        private List<ImportResult> ImportEpisodeDecision(List<ImportResult> importResults, ImportDecision importDecision, bool newDownload, DownloadClientItem downloadClientItem = null)
        {
            var localEpisode = importDecision.LocalItem as LocalEpisode;
            var oldFiles = new List<EpisodeFile>();

            try
            {
                //check if already imported
                if (importResults.Where(r => r.ImportDecision.LocalItem is LocalEpisode)
                                     .SelectMany(r => (r.ImportDecision.LocalItem as LocalEpisode).Episodes)
                                     .Select(e => e.Id)
                                     .Intersect(localEpisode.Episodes.Select(e => e.Id))
                                     .Any())
                {
                    importResults.Add(new ImportResult(importDecision, "Episode has already been imported"));
                    return importResults;
                }

                var episodeFile = new EpisodeFile();
                episodeFile.DateAdded = DateTime.UtcNow;
                episodeFile.SeriesId = localEpisode.Series.Id;
                episodeFile.Path = localEpisode.Path.CleanFilePath();
                episodeFile.Size = _diskProvider.GetFileSize(localEpisode.Path);
                episodeFile.Quality = localEpisode.Quality;
                episodeFile.MediaInfo = localEpisode.MediaInfo;
                episodeFile.SeasonNumber = localEpisode.SeasonNumber;
                episodeFile.Episodes = localEpisode.Episodes;
                if (localEpisode.ParsedEpisodeInfo != null)
                    episodeFile.ReleaseGroup = localEpisode.ParsedEpisodeInfo.ReleaseGroup;

                if (newDownload)
                {
                    bool copyOnly = downloadClientItem != null && downloadClientItem.IsReadOnly;

                    episodeFile.SceneName = GetSceneName(downloadClientItem, localEpisode);

                    var moveResult = _mediaFileUpgrader.UpgradeFile(episodeFile, localEpisode, copyOnly);
                    oldFiles = moveResult.OldFiles.Select(e => e as EpisodeFile).ToList();
                }
                else
                {
                    episodeFile.RelativePath = localEpisode.Series.Path.GetRelativePath(episodeFile.Path);
                }

                _mediaFileService.Add(episodeFile);
                importResults.Add(new ImportResult(importDecision));

                if (downloadClientItem != null)
                {
                    _eventAggregator.PublishEvent(new EpisodeImportedEvent(localEpisode, episodeFile, newDownload, downloadClientItem.DownloadClient, downloadClientItem.DownloadId));
                }
                else
                {
                    _eventAggregator.PublishEvent(new EpisodeImportedEvent(localEpisode, episodeFile, newDownload));
                }

                if (newDownload)
                {
                    _eventAggregator.PublishEvent(new EpisodeDownloadedEvent(localEpisode, episodeFile, oldFiles));
                }
            }
            catch (Exception e)
            {
                _logger.WarnException("Couldn't import episode " + localEpisode, e);
                importResults.Add(new ImportResult(importDecision, "Failed to import episode"));
            }
            return importResults;
        }

        private List<ImportResult> ImportMovieDecision(List<ImportResult> importResults, ImportDecision importDecision, bool newDownload, DownloadClientItem downloadClientItem = null)
        {
            var localMovie = importDecision.LocalItem as LocalMovie;
            var oldFile = new MovieFile();

            try
            {
                var movieFile = new MovieFile();
                movieFile.DateAdded = DateTime.UtcNow;
                movieFile.MovieId = localMovie.Movie.Id;
                movieFile.Path = localMovie.Path.CleanFilePath();
                movieFile.Size = _diskProvider.GetFileSize(localMovie.Path);
                movieFile.Quality = localMovie.Quality;
                movieFile.MediaInfo = localMovie.MediaInfo;
                movieFile.ReleaseGroup = localMovie.ParsedMovieInfo.ReleaseGroup;

                if (newDownload)
                {
                    bool copyOnly = downloadClientItem != null && downloadClientItem.IsReadOnly;

                    movieFile.SceneName = GetSceneName(downloadClientItem, localMovie);

                    var moveResult = _mediaFileUpgrader.UpgradeFile(movieFile, localMovie, copyOnly);
                    oldFile = moveResult.OldFiles.FirstOrDefault() as MovieFile;
                }
                else
                {
                    movieFile.RelativePath = localMovie.Movie.Path.GetRelativePath(movieFile.Path);
                }

                _mediaFileService.Add(movieFile);
                importResults.Add(new ImportResult(importDecision));

                if (downloadClientItem != null)
                {
                    _eventAggregator.PublishEvent(new MovieImportedEvent(localMovie, movieFile, newDownload, downloadClientItem.DownloadClient, downloadClientItem.DownloadId));
                }
                else
                {
                    _eventAggregator.PublishEvent(new MovieImportedEvent(localMovie, movieFile, newDownload));
                }

                if (newDownload)
                {
                    _eventAggregator.PublishEvent(new MovieDownloadedEvent(localMovie, movieFile, oldFile));
                }
            }
            catch (Exception e)
            {
                _logger.WarnException("Couldn't import movie " + localMovie, e);
                importResults.Add(new ImportResult(importDecision, "Failed to import movie"));
            }

            return importResults;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null)
        {
            var qualifiedImports = decisions.Where(c => c.Approved)
               .GroupBy(c => c.LocalItem.Media.Id, (i, s) => s
                   .OrderByDescending(c => c.LocalItem.Quality, new QualityModelComparer(s.First().LocalItem.Media.Profile))
                   .ThenByDescending(c => c.LocalItem.Size))
               .SelectMany(c => c)
               .ToList();

            var importResults = new List<ImportResult>();

            foreach (var importDecision in OrderDecision(qualifiedImports))
            {
                importResults = ImportDecision(importResults, importDecision, newDownload, downloadClientItem);
            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Reason).ToArray())));

            return importResults;
        }

        private string GetSceneName(DownloadClientItem downloadClientItem, LocalEpisode localEpisode)
        {
            if (downloadClientItem != null)
            {
                var title = Parser.Parser.RemoveFileExtension(downloadClientItem.Title);

                var parsedTitle = Parser.Parser.ParseTitle(title);

                if (parsedTitle != null && !parsedTitle.FullSeason)
                {
                    return title;
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(localEpisode.Path.CleanFilePath());

            if (SceneChecker.IsSeriesSceneTitle(fileName))
            {
                return fileName;
            }

            return null;
        }

        private string GetSceneName(DownloadClientItem downloadClientItem, LocalMovie localMovie)
        {
            if (downloadClientItem != null)
            {
                var title = Parser.Parser.RemoveFileExtension(downloadClientItem.Title);

                var parsedTitle = Parser.Parser.ParseMovieTitle(title);

                if (parsedTitle != null)
                {
                    return title;
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(localMovie.Path.CleanFilePath());

            if (SceneChecker.IsMovieSceneTitle(fileName))
            {
                return fileName;
            }

            return null;
        }
    }
}
