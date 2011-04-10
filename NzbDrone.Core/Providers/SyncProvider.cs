using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class SyncProvider : ISyncProvider
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly IMediaFileProvider _mediaFileProvider;
        private readonly NotificationProvider _notificationProvider;
        private readonly DiskProvider _diskProvider;

        private ProgressNotification _seriesSyncNotification;
        private Thread _seriesSyncThread;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SyncProvider(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
            IMediaFileProvider mediaFileProvider, NotificationProvider notificationProvider,
            DiskProvider diskProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _notificationProvider = notificationProvider;
            _diskProvider = diskProvider;
        }

        #region ISyncProvider Members

        public List<String> GetUnmappedFolders(string path)
        {
            Logger.Debug("Generating list of unmapped folders");
            if (String.IsNullOrEmpty(path))
                throw new InvalidOperationException("Invalid path provided");

            if (!_diskProvider.FolderExists(path))
            {
                Logger.Debug("Path supplied does not exist: {0}", path);
                return null;
            }

            var results = new List<String>();
            foreach (string seriesFolder in _diskProvider.GetDirectories(path))
            {
                var cleanPath = Parser.NormalizePath(new DirectoryInfo(seriesFolder).FullName);

                if (!_seriesProvider.SeriesPathExists(cleanPath))
                    results.Add(cleanPath);
            }

            Logger.Debug("{0} unmapped folders detected.", results.Count);
            return results;
        }

        #endregion

        public bool BeginUpdateNewSeries()
        {
            Logger.Debug("User has requested a scan of new series");
            if (_seriesSyncThread == null || !_seriesSyncThread.IsAlive)
            {
                Logger.Debug("Initializing background scan thread");
                _seriesSyncThread = new Thread(SyncNewSeries)
                {
                    Name = "SyncNewSeries",
                    Priority = ThreadPriority.Lowest
                };

                _seriesSyncThread.Start();
            }
            else
            {
                Logger.Warn("Series folder scan already in progress. Ignoring request.");

                //return false if sync was already running, then we can tell the user to try again later
                return false;
            }

            //return true if sync has started
            return true;
        }


        private void SyncNewSeries()
        {
            Logger.Info("Syncing new series");

            try
            {
                using (_seriesSyncNotification = new ProgressNotification("Series Scan"))
                {
                    _notificationProvider.Register(_seriesSyncNotification);

                    _seriesSyncNotification.CurrentStatus = "Finding New Series";
                    ScanSeries();

                    _seriesSyncNotification.CurrentStatus = "Series Scan Completed";
                    Logger.Info("Series folders scan has successfully completed.");
                    Thread.Sleep(3000);
                    _seriesSyncNotification.Status = ProgressNotificationStatus.Completed;
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
            }
        }

        private void ScanSeries()
        {


            var syncList = _seriesProvider.GetAllSeries().Where(s => s.LastInfoSync == null).ToList();
            if (syncList.Count == 0) return;

            _seriesSyncNotification.ProgressMax = syncList.Count;

            foreach (var currentSeries in syncList)
            {
                try
                {
                    _seriesSyncNotification.CurrentStatus = String.Format("Searching For: {0}", new DirectoryInfo(currentSeries.Path).Name);
                    var updatedSeries = _seriesProvider.UpdateSeriesInfo(currentSeries.SeriesId);

                    _seriesSyncNotification.CurrentStatus = String.Format("Downloading episode info For: {0}", updatedSeries.Title);
                    _episodeProvider.RefreshEpisodeInfo(updatedSeries.SeriesId);

                    _seriesSyncNotification.CurrentStatus = String.Format("Scanning series folder {0}", updatedSeries.Path);
                    _mediaFileProvider.Scan(_seriesProvider.GetSeries(updatedSeries.SeriesId));

                    //Todo: Launch Backlog search for this series _backlogProvider.StartSearch(mappedSeries.Id);
                }

                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);
                }
                _seriesSyncNotification.ProgressValue++;
            }

            //Keep scanning until there no more shows left.
            ScanSeries();
        }
    }
}
