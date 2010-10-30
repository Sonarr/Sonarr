using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers
{
    public class SyncProvider : ISyncProvider
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly IEpisodeProvider _episodeProvider;
        private readonly IMediaFileProvider _mediaFileProvider;
        private readonly INotificationProvider _notificationProvider;

        private ProgressNotification _seriesSyncNotification;
        private Thread _seriesSyncThread;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SyncProvider(ISeriesProvider seriesProvider, IEpisodeProvider episodeProvider, IMediaFileProvider mediaFileProvider, INotificationProvider notificationProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _notificationProvider = notificationProvider;
        }

        public void BeginSyncUnmappedFolders()
        {
            Logger.Debug("User has request series folder scan");
            if (_seriesSyncThread == null || !_seriesSyncThread.IsAlive)
            {
                Logger.Debug("Initializing background scan of series folder.");
                _seriesSyncThread = new Thread(SyncUnmappedFolders)
                {
                    Name = "SyncUnmappedFolders",
                    Priority = ThreadPriority.Lowest
                };

                _seriesSyncThread.Start();
            }
            else
            {
                Logger.Warn("Series folder scan already in progress. Ignoring request.");
            }
        }

        public void SyncUnmappedFolders()
        {
            Logger.Info("Starting Series folder scan");

            try
            {
                using (_seriesSyncNotification = new ProgressNotification("Series Scan"))
                {
                    _notificationProvider.Register(_seriesSyncNotification);
                    _seriesSyncNotification.CurrentStatus = "Analysing Folder";
                    var unmappedFolders = _seriesProvider.GetUnmappedFolders();
                    _seriesSyncNotification.ProgressMax = unmappedFolders.Count;

                    foreach (string seriesFolder in unmappedFolders)
                    {
                        try
                        {
                            _seriesSyncNotification.CurrentStatus = String.Format("Searching For: {0}", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(new DirectoryInfo(seriesFolder).Name));

                            Logger.Debug("Folder '{0}' isn't mapped in the database. Trying to map it.'", seriesFolder);
                            var mappedSeries = _seriesProvider.MapPathToSeries(seriesFolder);

                            if (mappedSeries == null)
                            {
                                Logger.Warn("Unable to find a matching series for '{0}'", seriesFolder);
                            }
                            else
                            {
                                //Check if series is mapped to another folder
                                if (_seriesProvider.GetSeries(mappedSeries.Id) == null)
                                {
                                    _seriesSyncNotification.CurrentStatus = String.Format("{0}: downloading series info...", mappedSeries.SeriesName);
                                    _seriesProvider.AddSeries(seriesFolder, mappedSeries);
                                    _episodeProvider.RefreshEpisodeInfo(mappedSeries.Id);
                                    _seriesSyncNotification.CurrentStatus = String.Format("{0}: finding episodes on disk...", mappedSeries.SeriesName);
                                    _mediaFileProvider.Scan(_seriesProvider.GetSeries(mappedSeries.Id));

                                }
                                else
                                {
                                    Logger.Warn("Folder '{0}' mapped to '{1}' which is already another folder assigned to it.'", seriesFolder, mappedSeries.SeriesName);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.ErrorException(e.Message, e);
                        }
                        _seriesSyncNotification.ProgressValue++;
                    }

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
    }
}
