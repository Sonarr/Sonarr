using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers
{
    public class PostDownloadProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex StatusRegex = new Regex(@"^_[\w_]*_", RegexOptions.Compiled);
        private readonly DiskProvider _diskProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IMoveEpisodeFiles _episodeFileMover;

        public PostDownloadProvider(DiskProvider diskProvider, DiskScanProvider diskScanProvider, ISeriesRepository seriesRepository, IMoveEpisodeFiles episodeFileMover)
        {
            _diskProvider = diskProvider;
            _diskScanProvider = diskScanProvider;
            _seriesRepository = seriesRepository;
            _episodeFileMover = episodeFileMover;
        }

        public PostDownloadProvider()
        {
        }

        public virtual void ProcessDropFolder(string dropFolder)
        {
            foreach (var subfolder in _diskProvider.GetDirectories(dropFolder))
            {
                try
                {
                    if (!_seriesRepository.SeriesPathExists(subfolder))
                    {
                        ProcessDownload(new DirectoryInfo(subfolder));
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while importing folder: " + subfolder, e);
                }
            }

            foreach(var videoFile in _diskScanProvider.GetVideoFiles(dropFolder, false))
            {
                try
                {
                    ProcessVideoFile(videoFile);
                }
                catch(Exception ex)
                {
                    Logger.ErrorException("An error has occurred while importing video file" + videoFile, ex);
                }
            }
        }

        public virtual void ProcessDownload(DirectoryInfo subfolderInfo)
        {
            if (subfolderInfo.Name.StartsWith("_") && _diskProvider.GetLastDirectoryWrite(subfolderInfo.FullName).AddMinutes(2) > DateTime.UtcNow)
            {
                Logger.Trace("[{0}] is too fresh. skipping", subfolderInfo.Name);
                return;
            }

            if (_diskProvider.IsFolderLocked(subfolderInfo.FullName))
            {
                Logger.Trace("[{0}] is currently locked by another process, skipping", subfolderInfo.Name);
                return;
            }

            string seriesName = Parser.ParseSeriesName(RemoveStatusFromFolderName(subfolderInfo.Name));
            var series = _seriesRepository.GetByTitle(seriesName);

            if (series == null)
            {
                Logger.Trace("Unknown Series on Import: {0}", subfolderInfo.Name);
                TagFolder(subfolderInfo, PostDownloadStatusType.UnknownSeries);
                return;
            }

            if (!_diskProvider.FolderExists(series.Path))
            {
                Logger.Warn("Series Folder doesn't exist: {0}", series.Path);
                return;
            }

            var size = _diskProvider.GetDirectorySize(subfolderInfo.FullName);
            var freeSpace = _diskProvider.FreeDiskSpace(series.Path);

            if (Convert.ToUInt64(size) > freeSpace)
            {
                Logger.Error("Not enough free disk space for series: {0}, {1}", series.Title, series.Path);
                return;
            }

            _diskScanProvider.CleanUpDropFolder(subfolderInfo.FullName);

            var importedFiles = _diskScanProvider.Scan(series, subfolderInfo.FullName);
            importedFiles.ForEach(file => _episodeFileMover.MoveEpisodeFile(file, true));

            //Delete the folder only if folder is small enough
            if (_diskProvider.GetDirectorySize(subfolderInfo.FullName) < Constants.IgnoreFileSize)
            {
                Logger.Trace("Episode(s) imported, deleting folder: {0}", subfolderInfo.Name);
                _diskProvider.DeleteFolder(subfolderInfo.FullName, true);
            }
            else
            {
                if (importedFiles.Count == 0)
                {
                    Logger.Trace("No Imported files: {0}", subfolderInfo.Name);
                    TagFolder(subfolderInfo, PostDownloadStatusType.ParseError);
                }
                else
                {
                    //Unknown Error Importing (Possibly a lesser quality than episode currently on disk)
                    Logger.Trace("Unable to import series (Unknown): {0}", subfolderInfo.Name);
                    TagFolder(subfolderInfo, PostDownloadStatusType.Unknown);
                }
            }
        }

        public virtual void ProcessVideoFile(string videoFile)
        {
            if (_diskProvider.GetLastFileWrite(videoFile).AddMinutes(2) > DateTime.UtcNow)
            {
                Logger.Trace("[{0}] is too fresh. skipping", videoFile);
                return;
            }

            if (_diskProvider.IsFileLocked(new FileInfo(videoFile)))
            {
                Logger.Trace("[{0}] is currently locked by another process, skipping", videoFile);
                return;
            }

            var seriesName = Parser.ParseSeriesName(Path.GetFileNameWithoutExtension(videoFile));
            var series = _seriesRepository.GetByTitle(seriesName);

            if (series == null)
            {
                Logger.Trace("Unknown Series on Import: {0}", videoFile);
                return;
            }

            if (!_diskProvider.FolderExists(series.Path))
            {
                Logger.Warn("Series Folder doesn't exist: {0}", series.Path);
                return;
            }

            var size = _diskProvider.GetSize(videoFile);
            var freeSpace = _diskProvider.FreeDiskSpace(series.Path);

            if (Convert.ToUInt64(size) > freeSpace)
            {
                Logger.Error("Not enough free disk space for series: {0}, {1}", series.Title, series.Path);
                return;
            }

            var episodeFile = _diskScanProvider.ImportFile(series, videoFile);
            if (episodeFile != null)
            {
                _episodeFileMover.MoveEpisodeFile(episodeFile, true);
            }
        }

        private void TagFolder(DirectoryInfo directory, PostDownloadStatusType status)
        {
            //Turning off tagging folder for now, to stop messing people's series folders.
            return;
            var target = GetTaggedFolderName(directory, status);

            if (!DiskProvider.PathEquals(target, directory.FullName))
            {
                Logger.Warn("Unable to download [{0}]. Status: {1}",directory.Name, status);
                _diskProvider.MoveDirectory(directory.FullName, target);
            }
            else
            {
                Logger.Debug("Unable to download [{0}], {1}", directory.Name, status);    
            }
        }

        public static string GetTaggedFolderName(DirectoryInfo directoryInfo, PostDownloadStatusType status)
        {
            if (status == PostDownloadStatusType.NoError)
                throw new InvalidOperationException("Can't tag a folder with a None-error status. " + status);

            string cleanName = RemoveStatusFromFolderName(directoryInfo.Name);
            string newName = string.Format("_{0}_{1}", status, cleanName);

            return Path.Combine(directoryInfo.Parent.FullName, newName);
        }

        public static string RemoveStatusFromFolderName(string folderName)
        {
            return StatusRegex.Replace(folderName, string.Empty);
        }
    }
}