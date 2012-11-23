using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class DiskScanProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] mediaExtentions = new[] { ".mkv", ".avi", ".wmv", ".mp4", ".mpg", ".mpeg", ".xvid", ".flv", ".mov", ".rm", ".rmvb", ".divx", ".dvr-ms", ".ts", ".ogm", ".m4v", ".strm" };
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly DownloadProvider _downloadProvider;
        private readonly SignalRProvider _signalRProvider;
        private readonly ConfigProvider _configProvider;
        private readonly RecycleBinProvider _recycleBinProvider;

        [Inject]
        public DiskScanProvider(DiskProvider diskProvider, EpisodeProvider episodeProvider,
                                SeriesProvider seriesProvider, MediaFileProvider mediaFileProvider,
                                ExternalNotificationProvider externalNotificationProvider, DownloadProvider downloadProvider,
                                SignalRProvider signalRProvider, ConfigProvider configProvider,
                                RecycleBinProvider recycleBinProvider)
        {
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
            _seriesProvider = seriesProvider;
            _mediaFileProvider = mediaFileProvider;
            _externalNotificationProvider = externalNotificationProvider;
            _downloadProvider = downloadProvider;
            _signalRProvider = signalRProvider;
            _configProvider = configProvider;
            _recycleBinProvider = recycleBinProvider;
        }

        public DiskScanProvider()
        {
        }

        /// <summary>
        ///   Scans the specified series folder for media files
        /// </summary>
        /// <param name = "series">The series to be scanned</param>
        public virtual List<EpisodeFile> Scan(Series series)
        {
            return Scan(series, series.Path);
        }

        /// <summary>
        ///   Scans the specified series folder for media files
        /// </summary>
        /// <param name = "series">The series to be scanned</param>
        /// <param name="path">Path to scan</param>
        public virtual List<EpisodeFile> Scan(Series series, string path)
        {
            _mediaFileProvider.CleanUpDatabase();

            if (!_diskProvider.FolderExists(path))
            {
                Logger.Warn("Series folder doesn't exist: {0}", path);
                return new List<EpisodeFile>();
            }

            if (_episodeProvider.GetEpisodeBySeries(series.SeriesId).Count == 0)
            {
                Logger.Debug("Series {0} has no episodes. skipping", series.Title);
                return new List<EpisodeFile>();
            }

            var seriesFile = _mediaFileProvider.GetSeriesFiles(series.SeriesId);
            CleanUp(seriesFile);

            var mediaFileList = GetVideoFiles(path);
            var importedFiles = new List<EpisodeFile>();

            foreach (var filePath in mediaFileList)
            {
                var file = ImportFile(series, filePath);
                if (file != null)
                {
                    importedFiles.Add(file);
                }
            }

            series.LastDiskSync = DateTime.Now;
            _seriesProvider.UpdateSeries(series);

            return importedFiles;
        }

        public virtual EpisodeFile ImportFile(Series series, string filePath)
        {
            Logger.Trace("Importing file to database [{0}]", filePath);

            if (_mediaFileProvider.Exists(filePath))
            {
                Logger.Trace("[{0}] already exists in the database. skipping.", filePath);
                return null;
            }

            long size = _diskProvider.GetSize(filePath);

            //Skip any file under 40MB - New samples don't even have sample in the name...
            if (size < Constants.IgnoreFileSize)
            {
                Logger.Trace("[{0}] appears to be a sample. skipping.", filePath);
                return null;
            }

            var parseResult = Parser.ParsePath(filePath);

            if (parseResult == null)
                return null;

            if (!_diskProvider.IsChildOfPath(filePath, series.Path))
                parseResult.SceneSource = true;

            parseResult.SeriesTitle = series.Title; //replaces the nasty path as title to help with logging
            parseResult.Series = series;

            var episodes = _episodeProvider.GetEpisodesByParseResult(parseResult);

            if (episodes.Count <= 0)
            {
                Logger.Debug("Can't find any matching episodes in the database. Skipping {0}", filePath);
                return null;
            }

            //Make sure this file is an upgrade for ALL episodes already on disk
            if (episodes.All(e => e.EpisodeFile == null || e.EpisodeFile.QualityWrapper <= parseResult.Quality))
            {
                Logger.Debug("Deleting the existing file(s) on disk to upgrade to: {0}", filePath);
                //Do the delete for files where there is already an episode on disk
                episodes.Where(e => e.EpisodeFile != null).Select(e => e.EpisodeFile.Path).Distinct().ToList().ForEach(p => _recycleBinProvider.DeleteFile(p));
            }

            else
            {
                //Skip this file because its not an upgrade
                Logger.Trace("This file isn't an upgrade for all episodes. Skipping {0}", filePath);
                return null;
            }

            var episodeFile = new EpisodeFile();
            episodeFile.DateAdded = DateTime.Now;
            episodeFile.SeriesId = series.SeriesId;
            episodeFile.Path = filePath.NormalizePath();
            episodeFile.Size = size;
            episodeFile.Quality = parseResult.Quality.Quality;
            episodeFile.Proper = parseResult.Quality.Proper;
            episodeFile.SeasonNumber = parseResult.SeasonNumber;
            episodeFile.SceneName = Path.GetFileNameWithoutExtension(filePath.NormalizePath());
            episodeFile.ReleaseGroup = parseResult.ReleaseGroup;
            var fileId = _mediaFileProvider.Add(episodeFile);

            //Link file to all episodes
            foreach (var ep in episodes)
            {
                ep.EpisodeFileId = fileId;
                ep.PostDownloadStatus = PostDownloadStatusType.NoError;
                _episodeProvider.UpdateEpisode(ep);
                Logger.Debug("Linking [{0}] > [{1}]", filePath, ep);
            }


            return episodeFile;
        }

        public virtual EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, bool newDownload = false)
        {
            if (episodeFile == null)
                throw new ArgumentNullException("episodeFile");

            var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
            var episodes = _episodeProvider.GetEpisodesByFileId(episodeFile.EpisodeFileId);
            string newFileName = _mediaFileProvider.GetNewFilename(episodes, series.Title, episodeFile.Quality, episodeFile.Proper, episodeFile);
            var newFile = _mediaFileProvider.CalculateFilePath(series, episodes.First().SeasonNumber, newFileName, Path.GetExtension(episodeFile.Path));

            //Only rename if existing and new filenames don't match
            if (DiskProvider.PathEquals(episodeFile.Path, newFile.FullName))
            {
                Logger.Debug("Skipping file rename, source and destination are the same: {0}", episodeFile.Path);
                return null;
            }

            _diskProvider.CreateDirectory(newFile.DirectoryName);

            Logger.Debug("Moving [{0}] > [{1}]", episodeFile.Path, newFile.FullName);
            _diskProvider.MoveFile(episodeFile.Path, newFile.FullName);

            //Wrapped in Try/Catch to prevent this from causing issues with remote NAS boxes, the move worked, which is more important.
            try
            {
                _diskProvider.InheritFolderPermissions(newFile.FullName);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Debug("Unable to apply folder permissions to: ", newFile.FullName);
                Logger.TraceException(ex.Message, ex);
            }

            episodeFile.Path = newFile.FullName;
            _mediaFileProvider.Update(episodeFile);

            var parseResult = Parser.ParsePath(episodeFile.Path);
            parseResult.Series = series;
            parseResult.Quality = new QualityModel{ Quality = episodeFile.Quality, Proper = episodeFile.Proper };
            parseResult.Episodes = episodes;

            var message = _downloadProvider.GetDownloadTitle(parseResult);

            if (newDownload)
            {
                _externalNotificationProvider.OnDownload(message, series);
                
                foreach(var episode in episodes)
                    _signalRProvider.UpdateEpisodeStatus(episode.EpisodeId, EpisodeStatusType.Ready, parseResult.Quality);
            }
            else
            {
                _externalNotificationProvider.OnRename(message, series);
            }

            return episodeFile;
        }

        /// <summary>
        ///   Removes files that no longer exist on disk from the database
        /// </summary>
        /// <param name = "files">list of files to verify</param>
        public virtual void CleanUp(IList<EpisodeFile> files)
        {
            foreach (var episodeFile in files)
            {
                try
                {
                    if(!_diskProvider.FileExists(episodeFile.Path))
                    {
                        Logger.Trace("File [{0}] no longer exists on disk. removing from db", episodeFile.Path);

                        //Set the EpisodeFileId for each episode attached to this file to 0
                        foreach(var episode in _episodeProvider.GetEpisodesByFileId(episodeFile.EpisodeFileId))
                        {
                            Logger.Trace("Setting EpisodeFileId for Episode: [{0}] to 0", episode.EpisodeId);
                            episode.EpisodeFileId = 0;
                            episode.Ignored = _configProvider.AutoIgnorePreviouslyDownloadedEpisodes;
                            episode.GrabDate = null;
                            episode.PostDownloadStatus = PostDownloadStatusType.Unknown;
                            _episodeProvider.UpdateEpisode(episode);
                        }

                        //Delete it from the DB
                        Logger.Trace("Removing EpisodeFile from DB.");
                        _mediaFileProvider.Delete(episodeFile.EpisodeFileId);
                    }
                }
                catch (Exception ex)
                {
                    var message = String.Format("Unable to cleanup EpisodeFile in DB: {0}", episodeFile.EpisodeFileId);
                    Logger.ErrorException(message, ex);
                }
            }
        }

        public virtual void CleanUpDropFolder(string path)
        {
            //Todo: We should rename files before importing them to prevent this issue from ever happening

            var filesOnDisk = GetVideoFiles(path);

            foreach(var file in filesOnDisk)
            {
                try
                {
                    var episodeFile = _mediaFileProvider.GetFileByPath(file);

                    if (episodeFile != null)
                    {
                        Logger.Trace("[{0}] was imported but not moved, moving it now", file);

                        MoveEpisodeFile(episodeFile, true);
                    }

                }
                catch (Exception ex)
                {
                    Logger.WarnException("Failed to move epiosde file from drop folder: " + file, ex);
                    throw;
                }
            }
        }

        public virtual List<string> GetVideoFiles(string path, bool allDirectories = true)
        {
            Logger.Debug("Scanning '{0}' for video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption);

            var mediaFileList = filesOnDisk.Where(c => mediaExtentions.Contains(Path.GetExtension(c).ToLower())).ToList();

            Logger.Trace("{0} video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList;
        }
    }
}