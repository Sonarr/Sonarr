using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers
{
    public class SyncProvider : ISyncProvider
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly IEpisodeProvider _episodeProvider;
        private readonly IMediaFileProvider _mediaFileProvider;
        private readonly INotificationProvider _notificationProvider;
        private readonly IDiskProvider _diskProvider;

        private ProgressNotification _seriesSyncNotification;
        private Thread _seriesSyncThread;
        private List<SeriesMappingModel> _syncList;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SyncProvider(ISeriesProvider seriesProvider, IEpisodeProvider episodeProvider,
            IMediaFileProvider mediaFileProvider, INotificationProvider notificationProvider,
            IDiskProvider diskProvider)
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

        public bool BeginSyncUnmappedFolders(List<SeriesMappingModel> unmapped)
        {
            Logger.Debug("User has requested series folder scan");
            if (_seriesSyncThread == null || !_seriesSyncThread.IsAlive)
            {
                Logger.Debug("Initializing background scan of series folder.");
                _seriesSyncThread = new Thread(SyncUnmappedFolders)
                {
                    Name = "SyncUnmappedFolders",
                    Priority = ThreadPriority.Lowest
                };

                _syncList = unmapped;
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

        public bool BeginAddNewSeries(string dir, int seriesId, string seriesName)
        {
            Logger.Debug("User has requested adding of new series");
            if (_seriesSyncThread == null || !_seriesSyncThread.IsAlive)
            {
                Logger.Debug("Initializing background add of of series folder.");
                _seriesSyncThread = new Thread(SyncUnmappedFolders)
                {
                    Name = "SyncUnmappedFolders",
                    Priority = ThreadPriority.Lowest
                };

                _syncList = new List<SeriesMappingModel>();

                var path = dir + Path.DirectorySeparatorChar + seriesName;

                //Create a directory for this new series
                _diskProvider.CreateDirectory(path);

                //Add it to the list so it will be processed
                _syncList.Add(new SeriesMappingModel { Path = path, TvDbId = seriesId });

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

        private void SyncUnmappedFolders()
        {
            Logger.Info("Starting Series folder scan");

            try
            {
                using (_seriesSyncNotification = new ProgressNotification("Series Scan"))
                {
                    _notificationProvider.Register(_seriesSyncNotification);
                    _seriesSyncNotification.CurrentStatus = "Analysing Folder";
                    _seriesSyncNotification.ProgressMax = _syncList.Count;

                    foreach (var seriesFolder in _syncList)
                    {
                        try
                        {
                            _seriesSyncNotification.CurrentStatus = String.Format("Searching For: {0}", new DirectoryInfo(seriesFolder.Path).Name);

                            if (_seriesProvider.SeriesPathExists(Parser.NormalizePath(seriesFolder.Path)))
                            {
                                Logger.Debug("Folder '{0}' is mapped in the database. Skipping.'", seriesFolder);
                                continue;
                            }

                            Logger.Debug("Folder '{0}' isn't mapped in the database. Trying to map it.'", seriesFolder);
                            var mappedSeries = _seriesProvider.MapPathToSeries(seriesFolder.TvDbId);

                            if (mappedSeries == null)
                            {
                                Logger.Warn("Invalid TVDB ID '{0}' Unable to map: '{1}'", seriesFolder.TvDbId, seriesFolder.Path);
                            }
                            else
                            {
                                //Check if series is mapped to another folder
                                if (_seriesProvider.GetSeries(mappedSeries.Id) == null)
                                {
                                    _seriesSyncNotification.CurrentStatus = String.Format("{0}: downloading series info...", mappedSeries.SeriesName);
                                    _seriesProvider.AddSeries(seriesFolder.Path, mappedSeries);
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
