using System;
using System.Web.Mvc;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class EpisodeController : Controller
    {
        private readonly JobController _jobProvider;
        private readonly MediaFileProvider _mediaFileProvider;

        public EpisodeController(JobController jobProvider, MediaFileProvider mediaFileProvider)
        {
            _jobProvider = jobProvider;
            _mediaFileProvider = mediaFileProvider;
        }

        public JsonResult Search(int episodeId)
        {
            _jobProvider.QueueJob(typeof(EpisodeSearchJob), new { EpisodeId = episodeId });
            return JsonNotificationResult.Queued("Episode search");
        }

        public JsonResult SearchSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(SeasonSearchJob), new { SeriesId = seriesId, SeasonNumber = seasonNumber });
            return JsonNotificationResult.Queued("Season search");

        }

        public JsonResult BacklogSeries(int seriesId)
        {
            _jobProvider.QueueJob(typeof(SeriesSearchJob), new { SeriesId = seriesId });
            return JsonNotificationResult.Queued("Series Backlog");

        }

        public JsonResult RenameSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(RenameSeasonJob), new { SeriesId = seriesId, SeasonNumber = seasonNumber });
            return JsonNotificationResult.Queued("Season rename");

        }

        public JsonResult RenameSeries(int seriesId)
        {
            _jobProvider.QueueJob(typeof(RenameSeriesJob), new { SeriesId = seriesId });
            return JsonNotificationResult.Queued("Series rename");
        }

        public JsonResult RenameAllSeries()
        {
            _jobProvider.QueueJob(typeof(RenameSeriesJob));
            return JsonNotificationResult.Queued("Series rename");
        }

        [HttpPost]
        public JsonResult ChangeEpisodeQuality(int episodeFileId, QualityTypes quality)
        {
            _mediaFileProvider.ChangeQuality(episodeFileId, quality);
            return Json("ok");
        }

        [HttpPost]
        public JsonResult ChangeSeasonQuality(int seriesId, int seasonNumber, QualityTypes quality)
        {
            _mediaFileProvider.ChangeQuality(seriesId, seasonNumber, quality);
            return Json("ok");
        }
    }
}