using System.Collections.Generic;
using System.Linq;
using System;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class RenameSeriesJob : IJob
    {
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly MetadataProvider _metadataProvider;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IEventAggregator _eventAggregator;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RenameSeriesJob(MediaFileProvider mediaFileProvider, DiskScanProvider diskScanProvider,
                                MetadataProvider metadataProvider,ISeriesRepository seriesRepository,IEventAggregator eventAggregator)
        {
            _mediaFileProvider = mediaFileProvider;
            _diskScanProvider = diskScanProvider;
            _metadataProvider = metadataProvider;
            _seriesRepository = seriesRepository;
            _eventAggregator = eventAggregator;
        }

        public string Name
        {
            get { return "Rename Series"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            List<Series> seriesToRename;

            if (options == null || options.SeriesId <= 0)
            {
                seriesToRename = _seriesRepository.All().ToList();
            }

            else
            {
                seriesToRename = new List<Series>{  _seriesRepository.Get((int)options.SeriesId) };
            }

            foreach(var series in seriesToRename)
            {
                notification.CurrentMessage = String.Format("Renaming episodes for '{0}'", series.Title);

                Logger.Debug("Getting episodes from database for series: {0}", series.OID);
                var episodeFiles = _mediaFileProvider.GetSeriesFiles(series.OID);

                if (episodeFiles == null || episodeFiles.Count == 0)
                {
                    Logger.Warn("No episodes in database found for series: {0}", series.OID);
                    return;
                }

                var newEpisodeFiles = new List<EpisodeFile>();
                var oldEpisodeFiles = new List<EpisodeFile>();

                foreach (var episodeFile in episodeFiles)
                {
                    try
                    {
                        var oldFile = new EpisodeFile(episodeFile);
                        var newFile = _diskScanProvider.MoveEpisodeFile(episodeFile);

                        if (newFile != null)
                        {
                            newEpisodeFiles.Add(newFile);
                            oldEpisodeFiles.Add(oldFile);
                        }
                    }

                    catch (Exception e)
                    {
                        Logger.WarnException("An error has occurred while renaming file", e);
                    }
                }

                //Remove & Create Metadata for episode files
                _metadataProvider.RemoveForEpisodeFiles(oldEpisodeFiles);
                _metadataProvider.CreateForEpisodeFiles(newEpisodeFiles);

                //Start AfterRename

                _eventAggregator.Publish(new SeriesRenamedEvent(series));

                notification.CurrentMessage = String.Format("Rename completed for {0}", series.Title);
            }
        }
    }
}