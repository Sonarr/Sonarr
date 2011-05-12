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
        private readonly IRepository _repository;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DeleteSeriesJob(IRepository repository, SeriesProvider seriesProvider)
        {
            _repository = repository;
            _seriesProvider = seriesProvider;
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
                var series = _repository.Single<Series>(seriesId);

                notification.CurrentMessage = String.Format("Beginning Delete of Series: {0}", series.Title);

                Logger.Debug("Deleting Series from DB {0}", series.Title);
                _repository.Delete<Series>(seriesId);

                Logger.Debug("Deleting History Items from DB for Series: {0}", series.SeriesId);
                var episodes = series.Episodes.Select(e => e.EpisodeId).ToList();
                episodes.ForEach(e => _repository.DeleteMany<History>(h => h.EpisodeId == e));

                Logger.Debug("Deleting EpisodeFiles from DB for Series: {0}", series.SeriesId);
                _repository.DeleteMany(series.EpisodeFiles);

                Logger.Debug("Deleting Episodes from DB for Series: {0}", series.SeriesId);
                _repository.DeleteMany(series.Episodes);

                Logger.Debug("Deleting Seasons from DB for Series: {0}", series.SeriesId);
                _repository.DeleteMany(series.Seasons);

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