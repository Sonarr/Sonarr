using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        EpisodeFile ImportFile(Series series, string filePath);
        string[] GetVideoFiles(string path, bool allDirectories = true);
    }

    public class DiskScanService : IDiskScanService, IExecute<DiskScanCommand>, IHandle<EpisodeInfoAddedEvent>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtensions = new[] { ".mkv", ".avi", ".wmv", ".mp4", ".mpg", ".mpeg", ".xvid", ".flv", ".mov", ".rm", ".rmvb", ".divx", ".dvr-ms", ".ts", ".ogm", ".m4v", ".strm" };
        private readonly IDiskProvider _diskProvider;
        private readonly ISeriesService _seriesService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IParsingService _parsingService;
        private readonly IMessageAggregator _messageAggregator;

        public DiskScanService(IDiskProvider diskProvider, ISeriesService seriesService, IMediaFileService mediaFileService, IVideoFileInfoReader videoFileInfoReader,
            IParsingService parsingService, IMessageAggregator messageAggregator)
        {
            _diskProvider = diskProvider;
            _seriesService = seriesService;
            _mediaFileService = mediaFileService;
            _videoFileInfoReader = videoFileInfoReader;
            _parsingService = parsingService;
            _messageAggregator = messageAggregator;
        }

        private void Scan(Series series)
        {
            if (!_diskProvider.FolderExists(series.Path))
            {
                Logger.Warn("Series folder doesn't exist: {0}", series.Path);
                return;
            }

            _messageAggregator.PublishCommand(new CleanMediaFileDb(series.Id));

            var mediaFileList = GetVideoFiles(series.Path);

            foreach (var filePath in mediaFileList)
            {
                try
                {
                    ImportFile(series, filePath);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Couldn't import file " + filePath, e);
                }
            }

            //Todo: Find the "best" episode file for all found episodes and import that one
            //Todo: Move the episode linking to here, instead of import (or rename import)
        }

        public EpisodeFile ImportFile(Series series, string filePath)
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

            var size = _diskProvider.GetFileSize(filePath);

            if (series.SeriesType == SeriesTypes.Daily || parsedEpisode.SeasonNumber > 0)
            {
                var runTime = _videoFileInfoReader.GetRunTime(filePath);
                if (size < Constants.IgnoreFileSize && runTime.TotalMinutes < 3)
                {
                    Logger.Trace("[{0}] appears to be a sample. skipping.", filePath);
                    return null;
                }
            }

            if (parsedEpisode.Episodes.Any(e => e.EpisodeFileId != 0 && e.EpisodeFile.Value.Quality > parsedEpisode.Quality))
            {
                Logger.Trace("This file isn't an upgrade for all episodes. Skipping {0}", filePath);
                return null;
            }

            var episodeFile = new EpisodeFile();
            episodeFile.DateAdded = DateTime.UtcNow;
            episodeFile.SeriesId = series.Id;
            episodeFile.Path = filePath.CleanPath();
            episodeFile.Size = size;
            episodeFile.Quality = parsedEpisode.Quality;
            episodeFile.SeasonNumber = parsedEpisode.SeasonNumber;
            episodeFile.SceneName = Path.GetFileNameWithoutExtension(filePath.CleanPath());
            episodeFile.Episodes = parsedEpisode.Episodes;

            //Todo: We shouldn't actually import the file until we confirm its the only one we want.
            //Todo: Separate episodeFile creation from importing (pass file to import to import)
            _mediaFileService.Add(episodeFile);
            return episodeFile;
        }

        public string[] GetVideoFiles(string path, bool allDirectories = true)
        {
            Logger.Debug("Scanning '{0}' for video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption);

            var mediaFileList = filesOnDisk.Where(c => MediaExtensions.Contains(Path.GetExtension(c).ToLower())).ToList();

            Logger.Trace("{0} video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList.ToArray();
        }

        public void Execute(DiskScanCommand message)
        {
            var seriesToScan = new List<Series>();

            if (message.SeriesId.HasValue)
            {
                seriesToScan.Add(_seriesService.GetSeries(message.SeriesId.Value));
            }
            else
            {
                seriesToScan.AddRange(_seriesService.GetAllSeries());
            }

            foreach (var series in seriesToScan)
            {
                try
                {
                    Scan(series);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Diskscan failed for " + series.Title, e);
                }
            }
        }

        public void Handle(EpisodeInfoAddedEvent message)
        {
            Scan(message.Series);
        }
    }
}