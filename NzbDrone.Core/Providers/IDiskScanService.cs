using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Providers
{
    public interface IDiskScanService
    {
        void Scan(Series series);
        EpisodeFile ImportFile(Series series, string filePath);
        string[] GetVideoFiles(string path, bool allDirectories = true);
    }

    public class DiskScanService : IDiskScanService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtensions = new[] { ".mkv", ".avi", ".wmv", ".mp4", ".mpg", ".mpeg", ".xvid", ".flv", ".mov", ".rm", ".rmvb", ".divx", ".dvr-ms", ".ts", ".ogm", ".m4v", ".strm" };
        private readonly DiskProvider _diskProvider;
        private readonly ICleanGhostFiles _ghostFileCleaner;
        private readonly IMediaFileService _mediaFileService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IParsingService _parsingService;

        public DiskScanService(DiskProvider diskProvider, ICleanGhostFiles ghostFileCleaner, IMediaFileService mediaFileService, IVideoFileInfoReader videoFileInfoReader,
            IParsingService parsingService)
        {
            _diskProvider = diskProvider;
            _ghostFileCleaner = ghostFileCleaner;
            _mediaFileService = mediaFileService;
            _videoFileInfoReader = videoFileInfoReader;
            _parsingService = parsingService;
        }

        public virtual void Scan(Series series)
        {
            if (!_diskProvider.FolderExists(series.Path))
            {
                Logger.Warn("Series folder doesn't exist: {0}", series.Path);
                return;
            }

            _ghostFileCleaner.RemoveNonExistingFiles(series.Id);

            var mediaFileList = GetVideoFiles(series.Path);

            foreach (var filePath in mediaFileList)
            {
                ImportFile(series, filePath);
            }

            //Todo: Find the "best" episode file for all found episodes and import that one
            //Todo: Move the episode linking to here, instead of import (or rename import)
        }

        public virtual EpisodeFile ImportFile(Series series, string filePath)
        {
            Logger.Trace("Importing file to database [{0}]", filePath);

            if (_mediaFileService.Exists(filePath))
            {
                Logger.Trace("[{0}] already exists in the database. skipping.", filePath);
                return null;
            }

            var parsedEpisode = _parsingService.GetEpisodes(filePath, series);

            if (parsedEpisode == null || !parsedEpisode.Episodes.Any())
            {
                return null;
            }

            var size = _diskProvider.GetSize(filePath);

            if (series.SeriesType == SeriesTypes.Daily || parsedEpisode.SeasonNumber > 0)
            {
                var runTime = _videoFileInfoReader.GetRunTime(filePath);
                if (size < Constants.IgnoreFileSize && runTime.TotalMinutes < 3)
                {
                    Logger.Trace("[{0}] appears to be a sample. skipping.", filePath);
                    return null;
                }
            }

            if (parsedEpisode.Episodes.Any(e => e.EpisodeFile != null && e.EpisodeFile.Quality > parsedEpisode.Quality))
            {
                Logger.Trace("This file isn't an upgrade for all episodes. Skipping {0}", filePath);
                return null;
            }

            var episodeFile = new EpisodeFile();
            episodeFile.DateAdded = DateTime.Now;
            episodeFile.SeriesId = series.Id;
            episodeFile.Path = filePath.CleanPath();
            episodeFile.Size = size;
            episodeFile.Quality = parsedEpisode.Quality;
            episodeFile.SeasonNumber = parsedEpisode.SeasonNumber;
            episodeFile.SceneName = Path.GetFileNameWithoutExtension(filePath.CleanPath());

            //Todo: We shouldn't actually import the file until we confirm its the only one we want.
            //Todo: Separate episodeFile creation from importing (pass file to import to import)
            _mediaFileService.Add(episodeFile);
            return episodeFile;
        }

        public virtual string[] GetVideoFiles(string path, bool allDirectories = true)
        {
            Logger.Debug("Scanning '{0}' for video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption);

            var mediaFileList = filesOnDisk.Where(c => MediaExtensions.Contains(Path.GetExtension(c).ToLower())).ToList();

            Logger.Trace("{0} video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList.ToArray();
        }
    }
}