using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class PostDownloadProvider
    {
        //Used to perform Post Download Processing (Started by PostDownloadScanJob)

        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly List<PostDownloadInfoModel> InfoList = new List<PostDownloadInfoModel>();
            
        [Inject]
        public PostDownloadProvider(ConfigProvider configProvider, DiskProvider diskProvider,
                                    DiskScanProvider diskScanProvider, SeriesProvider seriesProvider,
                                    EpisodeProvider episodeProvider)
        {
            _configProvider = configProvider;
            _diskProvider = diskProvider;
            _diskScanProvider = diskScanProvider;
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
        }

        PostDownloadProvider()
        {
            
        }

        public virtual void ScanDropFolder(ProgressNotification notification)
        {
            var dropFolder = _configProvider.SabDropDirectory;

            if (String.IsNullOrWhiteSpace(dropFolder))
            {
                Logger.Debug("No drop folder is defined. Skipping.");
                return;
            }

            if (!_diskProvider.FolderExists(dropFolder))
            {
                Logger.Warn("Unable to Scan for New Downloads - folder Doesn't exist: [{0}]", dropFolder);
                return;
            }

            ProcessDropFolder(dropFolder);
        }

        public virtual void ProcessDropFolder(string dropFolder)
        {
            foreach (var subfolder in _diskProvider.GetDirectories(dropFolder))
            {
                try
                {
                    var subfolderInfo = new DirectoryInfo(subfolder);

                    if (subfolderInfo.Name.StartsWith("_UNPACK_"))
                    {
                        ProcessFailedOrUnpackingDownload(subfolderInfo, PostDownloadStatusType.Unpacking);
                        Logger.Debug("Folder [{0}] is still being unpacked. skipping.", subfolder);
                        continue;
                    }

                    if (subfolderInfo.Name.StartsWith("_FAILED_"))
                    {
                        ProcessFailedOrUnpackingDownload(subfolderInfo, PostDownloadStatusType.Failed);
                        Logger.Debug("Folder [{0}] is marked as failed. skipping.", subfolder);
                        continue;
                    }

                    if (subfolderInfo.Name.StartsWith("_NzbDrone_"))
                    {
                        if (subfolderInfo.Name.StartsWith("_NzbDrone_InvalidSeries_"))
                            ReProcessDownload(new PostDownloadInfoModel{ Name = subfolderInfo.FullName, Status = PostDownloadStatusType.InvalidSeries });

                        else if (subfolderInfo.Name.StartsWith("_NzbDrone_ParseError_"))
                            ReProcessDownload(new PostDownloadInfoModel { Name = subfolderInfo.FullName, Status = PostDownloadStatusType.ParseError });

                        else
                            ReProcessDownload(new PostDownloadInfoModel { Name = subfolderInfo.FullName, Status = PostDownloadStatusType.Unknown });

                        continue;
                    }

                    //Process a successful download
                    ProcessDownload(subfolderInfo);
                }

                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while importing " + subfolder, e);
                }
            }
        }

        public virtual void ProcessDownload(DirectoryInfo subfolderInfo)
        {
            //Parse the Folder name
            var seriesName = Parser.ParseSeriesName(subfolderInfo.Name);
            var series = _seriesProvider.FindSeries(seriesName);

            if (series == null)
            {
                Logger.Warn("Unable to Import new download [{0}], series doesn't exist in database.", subfolderInfo.FullName);

                //Rename the Directory so it's not processed again.
                _diskProvider.MoveDirectory(subfolderInfo.FullName,
                                            Path.Combine(subfolderInfo.Parent.FullName,
                                                         "_NzbDrone_InvalidSeries_" + subfolderInfo.Name));
                return;
            }

            var importedFiles = _diskScanProvider.Scan(series, subfolderInfo.FullName);
            importedFiles.ForEach(file => _diskScanProvider.MoveEpisodeFile(file, true));

            //Delete the folder only if folder is small enough
            if (_diskProvider.GetDirectorySize(subfolderInfo.FullName) < 10.Megabytes())
                _diskProvider.DeleteFolder(subfolderInfo.FullName, true);

            //Otherwise rename the folder to say it was already processed once by NzbDrone
            else
            {
                if (importedFiles.Count == 0)
                {
                    Logger.Warn("Unable to Import new download [{0}], unable to parse episode file(s).", subfolderInfo.FullName);
                    _diskProvider.MoveDirectory(subfolderInfo.FullName,
                                                Path.Combine(subfolderInfo.Parent.FullName,
                                                             "_NzbDrone_ParseError_" + subfolderInfo.Name));
                }

                //Unknown Error Importing (Possibly a lesser quality than episode currently on disk)
                else
                {
                    Logger.Warn("Unable to Import new download [{0}].", subfolderInfo.FullName);

                    _diskProvider.MoveDirectory(subfolderInfo.FullName,
                                                Path.Combine(subfolderInfo.Parent.FullName,
                                                             "_NzbDrone_" + subfolderInfo.Name));
                }
            }
        }

        public virtual void ProcessFailedOrUnpackingDownload(DirectoryInfo directoryInfo, PostDownloadStatusType postDownloadStatus)
        {
            //Check to see if its already in InfoList, if it is, check if enough time has passed to process
            if (InfoList.Any(i => i.Name == directoryInfo.FullName))
            {
                var model = InfoList.Single(i => i.Name == directoryInfo.FullName);

                //Process if 30 minutes has passed
                if (model.Added > DateTime.Now.AddMinutes(30))
                    ReProcessDownload(model);

                //If everything processed successfully up until now, remove it from InfoList
                InfoList.Remove(model);
                return;
            }
                
            //Add to InfoList for possible later processing
            InfoList.Add(new PostDownloadInfoModel{ Name = directoryInfo.FullName,
                                                    Added = DateTime.Now,
                                                    Status = postDownloadStatus 
                                                   });

            //Remove the first 8 characters of the folder name (removes _UNPACK_ or _FAILED_) before processing
            var parseResult = Parser.ParseTitle(directoryInfo.Name.Substring(8));
            parseResult.Series = _seriesProvider.FindSeries(parseResult.CleanTitle);

            var episodeIds = new List<int>();

            if (parseResult.EpisodeNumbers.Count == 0 && parseResult.FullSeason)
                episodeIds =
                    _episodeProvider.GetEpisodesBySeason(parseResult.Series.SeriesId, parseResult.SeasonNumber)
                    .Select(e => e.EpisodeId).ToList();

            else
                episodeIds = _episodeProvider.GetEpisodesByParseResult(parseResult).Select(e => e.EpisodeId).ToList();

            _episodeProvider.SetPostDownloadStatus(episodeIds, postDownloadStatus);
        }

        public virtual void ReProcessDownload(PostDownloadInfoModel model)
        {
            var directoryInfo = new DirectoryInfo(model.Name);
            var newName = Path.Combine(directoryInfo.Parent.FullName, directoryInfo.Name.Substring(GetPrefixLength(model.Status)));

            _diskProvider.MoveDirectory(directoryInfo.FullName, newName);

            directoryInfo = new DirectoryInfo(newName);

            ProcessDownload(directoryInfo);
        }

        public int GetPrefixLength(PostDownloadStatusType postDownloadStatus)
        {
            //_UNPACK_ & _FAILED_ have a length of 8
            if (postDownloadStatus == PostDownloadStatusType.Unpacking || postDownloadStatus == PostDownloadStatusType.Failed)
                return 8;

            //_NzbDrone_InvalidSeries_ - Length = 24
            if (postDownloadStatus == PostDownloadStatusType.InvalidSeries)
                return 24;

            //_NzbDrone_ParseError_ - Length = 
            if (postDownloadStatus == PostDownloadStatusType.ParseError)
                return 21;

            //_NzbDrone_ - Length = 10
            if (postDownloadStatus == PostDownloadStatusType.Unknown)
                return 10;

            //Default to zero
            return 0;
        }
    }
}
