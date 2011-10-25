using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class PostDownloadProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex StatusRegex = new Regex(@"^_[\w_]*_", RegexOptions.Compiled);
        private readonly DiskProvider _diskProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly SeriesProvider _seriesProvider;

        [Inject]
        public PostDownloadProvider(DiskProvider diskProvider, DiskScanProvider diskScanProvider,
                                    SeriesProvider seriesProvider)
        {
            _diskProvider = diskProvider;
            _diskScanProvider = diskScanProvider;
            _seriesProvider = seriesProvider;
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
                    ProcessDownload(new DirectoryInfo(subfolder));
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while importing folder" + subfolder, e);
                }
            }
        }

        public virtual void ProcessDownload(DirectoryInfo subfolderInfo)
        {
            if (subfolderInfo.Name.StartsWith("_") && subfolderInfo.LastWriteTimeUtc.AddMinutes(1) > DateTime.UtcNow)
            {
                Logger.Trace("[{0}] is too fresh. skipping", subfolderInfo.Name);
                return;
            }

            string seriesName = Parser.ParseSeriesName(RemoveStatusFromFolderName(subfolderInfo.Name));
            var series = _seriesProvider.FindSeries(seriesName);

            if (series == null)
            {
                Logger.Warn("Unable to Import new download [{0}], Can't find matching series in database.",
                            subfolderInfo.Name);
                TagFolder(subfolderInfo, PostDownloadStatusType.UnknownSeries);
                return;
            }

            var importedFiles = _diskScanProvider.Scan(series, subfolderInfo.FullName);
            importedFiles.ForEach(file => _diskScanProvider.MoveEpisodeFile(file, true));

            //Delete the folder only if folder is small enough
            if (_diskProvider.GetDirectorySize(subfolderInfo.FullName) < 10.Megabytes())
            {
                _diskProvider.DeleteFolder(subfolderInfo.FullName, true);
            }
            else
            {
                if (importedFiles.Count == 0)
                {
                    Logger.Warn("Unable to Import new download [{0}], no importable files were found.",
                                subfolderInfo.Name);
                    TagFolder(subfolderInfo, PostDownloadStatusType.ParseError);
                }
                else
                {
                    //Unknown Error Importing (Possibly a lesser quality than episode currently on disk)
                    Logger.Warn("Unable to Import new download [{0}].", subfolderInfo.Name);
                    TagFolder(subfolderInfo, PostDownloadStatusType.Unknown);
                }
            }
        }

        private void TagFolder(DirectoryInfo directory, PostDownloadStatusType status)
        {
            var target = GetTaggedFolderName(directory, status);

            if (!String.Equals(Parser.NormalizePath(target), Parser.NormalizePath(directory.FullName), StringComparison.InvariantCultureIgnoreCase))
            {
                _diskProvider.MoveDirectory(directory.FullName, target);
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