using System;
using System.Web.Mvc;
using NzbDrone.Core.Jobs;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class EpisodeController : Controller
    {
        private readonly JobProvider _jobProvider;

        public EpisodeController(JobProvider jobProvider)
        {
            _jobProvider = jobProvider;
        }

        public JsonResult Search(int episodeId)
        {
            _jobProvider.QueueJob(typeof(EpisodeSearchJob), episodeId);
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult SearchSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(SeasonSearchJob), seriesId, seasonNumber);
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult BacklogSeries(int seriesId)
        {
            _jobProvider.QueueJob(typeof(SeriesSearchJob), seriesId);
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult RenameSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(RenameSeasonJob), seriesId, seasonNumber);
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult RenameEpisodes(int seriesId)
        {
            _jobProvider.QueueJob(typeof(RenameSeriesJob), seriesId);
            return JsonNotificationResult.Info("Queued");
        }
    }
}