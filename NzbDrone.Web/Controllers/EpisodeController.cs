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
            return JsonNotificationResult.Queued("Episode search");
        }

        public JsonResult SearchSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(SeasonSearchJob), seriesId, seasonNumber);
            return JsonNotificationResult.Queued("Season search");

        }

        public JsonResult BacklogSeries(int seriesId)
        {
            _jobProvider.QueueJob(typeof(SeriesSearchJob), seriesId);
            return JsonNotificationResult.Queued("Series Backlog");

        }

        public JsonResult RenameSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(RenameSeasonJob), seriesId, seasonNumber);
            return JsonNotificationResult.Queued("Season rename");

        }

        public JsonResult RenameSeries(int seriesId)
        {
            _jobProvider.QueueJob(typeof(RenameSeriesJob), seriesId);
            return JsonNotificationResult.Queued("Series rename");
        }

        public JsonResult RenameAllSeries()
        {
            _jobProvider.QueueJob(typeof(RenameSeriesJob));
            return JsonNotificationResult.Queued("Series rename");
        }
    }
}