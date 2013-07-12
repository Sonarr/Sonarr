using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        string[] GetVideoFiles(string path, bool allDirectories = true);
    }

    public class DiskScanService : IDiskScanService, IExecute<DiskScanCommand>, IHandle<EpisodeInfoAddedEvent>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtensions = new[] { ".mkv", ".avi", ".wmv", ".mp4", ".mpg", ".mpeg", ".xvid", ".flv", ".mov", ".rm", ".rmvb", ".divx", ".dvr-ms", ".ts", ".ogm", ".m4v", ".strm" };
        private readonly IDiskProvider _diskProvider;
        private readonly ISeriesService _seriesService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly IMessageAggregator _messageAggregator;

        public DiskScanService(IDiskProvider diskProvider,
                                ISeriesService seriesService,
                                IMakeImportDecision importDecisionMaker, 
                                IImportApprovedEpisodes importApprovedEpisodes,
                                IMessageAggregator messageAggregator)
        {
            _diskProvider = diskProvider;
            _seriesService = seriesService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _messageAggregator = messageAggregator;
        }

        private void Scan(Series series)
        {
            _messageAggregator.PublishCommand(new CleanMediaFileDb(series.Id));
            
            if (!_diskProvider.FolderExists(series.Path))
            {
                Logger.Trace("Series folder doesn't exist: {0}", series.Path);
                return;
            }

            var mediaFileList = GetVideoFiles(series.Path);

            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, series);
            _importApprovedEpisodes.Import(decisions);
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
                    Logger.ErrorException("Disk scan failed for " + series, e);
                }
            }
        }

        public void Handle(EpisodeInfoAddedEvent message)
        {
            Scan(message.Series);
        }
    }
}