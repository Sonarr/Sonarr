using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class DiskScanProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtentions = new[] { ".mkv", ".avi", ".wmv", ".mp4", ".mpg", ".mpeg", ".xvid", ".flv", ".mov", ".vob", ".ts", ".rm", ".rmvb", ".xvid", ".dvr-ms" };
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly SabProvider _sabProvider;

        [Inject]
        public DiskScanProvider(DiskProvider diskProvider, EpisodeProvider episodeProvider,
                                SeriesProvider seriesProvider, MediaFileProvider mediaFileProvider,
                                ExternalNotificationProvider externalNotificationProvider, SabProvider sabProvider)
        {
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
            _seriesProvider = seriesProvider;
            _mediaFileProvider = mediaFileProvider;
            _externalNotificationProvider = externalNotificationProvider;
            _sabProvider = sabProvider;
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
            _mediaFileProvider.DeleteOrphaned();
            _mediaFileProvider.RepairLinks();

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
                    importedFiles.Add(file);
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

            //If Size is less than 40MB and contains sample. Check for Size to ensure its not an episode with sample in the title
            if (size < 40.Megabytes() && filePath.ToLower().Contains("sample"))
            {
                Logger.Trace("[{0}] appears to be a sample. skipping.", filePath);
                return null;
            }

            var parseResult = Parser.ParsePath(filePath);

            if (parseResult == null)
                return null;

            parseResult.CleanTitle = series.Title; //replaces the nasty path as title to help with logging
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
                episodes.Where(e => e.EpisodeFile != null).Select(e => e.EpisodeFile.Path).Distinct().ToList().ForEach(p => _diskProvider.DeleteFile(p));
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
            episodeFile.Path = Parser.NormalizePath(filePath);
            episodeFile.Size = size;
            episodeFile.Quality = parseResult.Quality.QualityType;
            episodeFile.Proper = parseResult.Quality.Proper;
            episodeFile.SeasonNumber = parseResult.SeasonNumber;
            var fileId = _mediaFileProvider.Add(episodeFile);

            //Link file to all episodes
            foreach (var ep in episodes)
            {
                ep.EpisodeFileId = fileId;
                ep.PostDownloadStatus = PostDownloadStatusType.Processed;
                _episodeProvider.UpdateEpisode(ep);
                Logger.Debug("Linking [{0}] > [{1}]", filePath, ep);
            }


            return episodeFile;
        }

        public virtual bool MoveEpisodeFile(EpisodeFile episodeFile, bool newDownload = false)
        {
            if (episodeFile == null)
                throw new ArgumentNullException("episodeFile");

            var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
            var episodes = _episodeProvider.GetEpisodesByFileId(episodeFile.EpisodeFileId);
            string newFileName = _mediaFileProvider.GetNewFilename(episodes, series.Title, episodeFile.Quality);
            var newFile = _mediaFileProvider.CalculateFilePath(series, episodes.First().SeasonNumber, newFileName, Path.GetExtension(episodeFile.Path));

            //Ensure the folder Exists before trying to move it (No error is thrown if the folder already exists)
            _diskProvider.CreateDirectory(newFile.DirectoryName);

            //Do the rename
            Logger.Debug("Moving [{0}] > [{1}]", episodeFile.Path, newFile.FullName);
            _diskProvider.MoveFile(episodeFile.Path, newFile.FullName);

            //Make the file inherit parent permissions
            _diskProvider.InheritFolderPermissions(newFile.FullName);

            //Update the filename in the DB
            episodeFile.Path = newFile.FullName;
            _mediaFileProvider.Update(episodeFile);

            //ExternalNotification
            var parseResult = Parser.ParsePath(episodeFile.Path);
            parseResult.Series = series;

            var message = _sabProvider.GetSabTitle(parseResult);
            
            if (newDownload)
                _externalNotificationProvider.OnDownload(message, series);

            else
                _externalNotificationProvider.OnRename(message, series);

            return true;
        }

        /// <summary>
        ///   Removes files that no longer exist on disk from the database
        /// </summary>
        /// <param name = "files">list of files to verify</param>
        public virtual void CleanUp(IList<EpisodeFile> files)
        {
            foreach (var episodeFile in files)
            {
                if (!_diskProvider.FileExists(episodeFile.Path))
                {
                    Logger.Trace("File [{0}] no longer exists on disk. removing from db", episodeFile.Path);

                    //Set the EpisodeFileId for each episode attached to this file to 0
                    foreach (var episode in _episodeProvider.GetEpisodesByFileId(episodeFile.EpisodeFileId))
                    {
                        episode.EpisodeFileId = 0;
                        _episodeProvider.UpdateEpisode(episode);
                    }

                    //Delete it from the DB
                    _mediaFileProvider.Delete(episodeFile.EpisodeFileId);
                }
            }
        }


        private List<string> GetVideoFiles(string path)
        {
            Logger.Debug("Scanning '{0}' for episodes", path);

            var filesOnDisk = _diskProvider.GetFiles(path, SearchOption.AllDirectories);

            var mediaFileList = filesOnDisk.Where(c => MediaExtentions.Contains(Path.GetExtension(c).ToLower())).ToList();

            Logger.Trace("{0} video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList;
        }
    }
}