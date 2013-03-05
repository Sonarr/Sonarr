using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Jobs.Implementations
{
    /// <summary>
    /// This job processes newly added jobs by downloading their info
    /// from TheTVDB.org and doing a disk scan. this job should only 
    /// </summary>
    public class ImportNewSeriesJob : IJob
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IMediaFileService _mediaFileService;
        private readonly UpdateInfoJob _updateInfoJob;
        private readonly DiskScanJob _diskScanJob;
        private readonly ISeasonRepository _seasonRepository;
        private readonly XemUpdateJob _xemUpdateJob;
        private readonly ISeriesRepository _seriesRepository;
        private readonly ISeasonService _seasonService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<int> _attemptedSeries;

        public ImportNewSeriesJob(ISeriesService seriesService, IEpisodeService episodeService,
                                    IMediaFileService mediaFileService, UpdateInfoJob updateInfoJob,
                                    DiskScanJob diskScanJob,
                                    ISeasonRepository seasonRepository, XemUpdateJob xemUpdateJob, ISeriesRepository seriesRepository, ISeasonService seasonService)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _mediaFileService = mediaFileService;
            _updateInfoJob = updateInfoJob;
            _diskScanJob = diskScanJob;
            _seasonRepository = seasonRepository;
            _xemUpdateJob = xemUpdateJob;
            _seriesRepository = seriesRepository;
            _seasonService = seasonService;
        }

        public string Name
        {
            get { return "Import New Series"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(6); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            _attemptedSeries = new List<int>();
            ScanSeries(notification);
        }

        private void ScanSeries(ProgressNotification notification)
        {
            var syncList = _seriesRepository.All().Where(s => s.LastInfoSync == null && !_attemptedSeries.Contains(s.Id)).ToList();
            if (syncList.Count == 0)
            {
                return;
            }

            foreach (var currentSeries in syncList)
            {
                try
                {
                    _attemptedSeries.Add(((ModelBase)currentSeries).Id);
                    notification.CurrentMessage = String.Format("Searching for '{0}'", new DirectoryInfo(currentSeries.Path).Name);

                    _updateInfoJob.Start(notification, new { SeriesId = ((ModelBase)currentSeries).Id });
                    _diskScanJob.Start(notification, new { SeriesId = ((ModelBase)currentSeries).Id });

                    var updatedSeries = _seriesRepository.Get(((ModelBase)currentSeries).Id);
                    AutoIgnoreSeasons(((ModelBase)updatedSeries).Id);

                    //Get Scene Numbering if applicable
                    _xemUpdateJob.Start(notification, new { SeriesId = ((ModelBase)updatedSeries).Id });

                    notification.CurrentMessage = String.Format("{0} was successfully imported", updatedSeries.Title);
                }

                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);
                }
            }

            //Keep scanning until there no more shows left.
            ScanSeries(notification);
        }

        public void AutoIgnoreSeasons(int seriesId)
        {
            //Todo: Need to convert this over to ObjectDb
            return;
            var episodeFiles = _mediaFileService.GetFilesBySeries(seriesId);

            if (episodeFiles.Count() != 0)
            {
                var seasons = _seasonRepository.GetSeasonNumbers(seriesId);
                var currentSeasons = seasons.Max();

                foreach (var season in seasons)
                {
                    if (season != currentSeasons && !episodeFiles.Any(e => e.SeasonNumber == season))
                    {
                        _seasonService.SetIgnore(seriesId, season, true);
                    }
                }
            }
        }
    }
}