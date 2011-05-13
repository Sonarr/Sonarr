using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class DeleteSeriesJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly SeasonProvider _seasonProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly HistoryProvider _historyProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DeleteSeriesJob(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
            EpisodeProvider episodeProvider, MediaFileProvider mediaFileProvider,
            HistoryProvider historyProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _historyProvider = historyProvider;
        }

        public string Name
        {
            get { return "Delete Series"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            DeleteSeries(notification, targetId);
        }

        private void DeleteSeries(ProgressNotification notification, int seriesId)
        {
            Logger.Warn("Deleting Series [{0}]", seriesId);

            try
            {
                var series = _seriesProvider.GetSeries(seriesId);

                notification.CurrentMessage = String.Format("Beginning Delete of Series: {0}", series.Title);

                Logger.Debug("Deleting Series from DB {0}", series.Title);
                _seriesProvider.DeleteSeries(seriesId);

                Logger.Debug("Deleting History Items from DB for Series: {0}", series.SeriesId);
                series.Episodes.ForEach(e => _historyProvider.DeleteForEpisode(e.EpisodeId));

                Logger.Debug("Deleting EpisodeFiles from DB for Series: {0}", series.SeriesId);
                series.EpisodeFiles.ForEach(f => _mediaFileProvider.DeleteFromDb(f.EpisodeFileId));

                Logger.Debug("Deleting Episodes from DB for Series: {0}", series.SeriesId);
                series.Episodes.ForEach(e => _episodeProvider.DeleteEpisode(e.EpisodeId));

                Logger.Debug("Deleting Seasons from DB for Series: {0}", series.SeriesId);
                series.Seasons.ForEach(s => _seasonProvider.DeleteSeason(s.SeasonId));

                notification.CurrentMessage = String.Format("Successfully deleted Series: {0}", series.Title);
                Logger.Info("Successfully deleted Series [{0}]", seriesId);
            }
            catch (Exception e)
            {
                Logger.ErrorException("An error has occurred while deleting series: " + seriesId, e);
                throw;
            }
        }
    }
}