using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        string[] GetVideoFiles(string path, bool allDirectories = true);
    }

    public class DiskScanService :
        IDiskScanService,
        IHandle<SeriesUpdatedEvent>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HashSet<string> MediaExtensions = new HashSet<string> { ".mkv", ".avi", ".wmv", ".mp4", ".mpg", ".mpeg", ".xvid", ".flv", ".mov", ".rm", ".rmvb", ".divx", ".dvr-ms", ".ts", ".ogm", ".m4v", ".strm", ".iso", "m2ts" };
        private readonly IDiskProvider _diskProvider;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly IMessageAggregator _messageAggregator;

        public DiskScanService(IDiskProvider diskProvider,
                                IMakeImportDecision importDecisionMaker,
                                IImportApprovedEpisodes importApprovedEpisodes,
                                IMessageAggregator messageAggregator)
        {
            _diskProvider = diskProvider;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _messageAggregator = messageAggregator;
        }

        private void Scan(Series series)
        {
            _messageAggregator.PublishCommand(new CleanMediaFileDb(series.Id));

            if (!_diskProvider.FolderExists(series.Path))
            {
                Logger.Debug("Series folder doesn't exist: {0}", series.Path);
                return;
            }

            var mediaFileList = GetVideoFiles(series.Path);

            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, series, false);
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

        public void Handle(SeriesUpdatedEvent message)
        {
            Scan(message.Series);
        }
    }
}