using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
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

        [HttpPost]
        public JsonResult Search(int episodeId)
        {
            _jobProvider.QueueJob(typeof(EpisodeSearchJob), episodeId);
            return new JsonResult { Data = "ok" };
        }

        [HttpPost]
        public JsonResult SearchSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(SeasonSearchJob), seriesId, seasonNumber);
            return new JsonResult { Data = "ok" };
        }

        public JsonResult BacklogSeries(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(SeriesSearchJob), seriesId);

            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult Rename(int episodeFileId)
        {
            _jobProvider.QueueJob(typeof(RenameEpisodeJob), episodeFileId);

            return new JsonResult { Data = "ok" };
        }

        public JsonResult RenameSeason(int seriesId, int seasonNumber)
        {
            _jobProvider.QueueJob(typeof(RenameSeasonJob), seriesId, seasonNumber);

            return new JsonResult { Data = "ok" };
        }

        public JsonResult RenameEpisodes(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(RenameSeriesJob), seriesId);

            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}