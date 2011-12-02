using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class HistoryController : Controller
    {
        private readonly HistoryProvider _historyProvider;
        private readonly JobProvider _jobProvider;

        public HistoryController(HistoryProvider historyProvider, JobProvider jobProvider)
        {
            _historyProvider = historyProvider;
            _jobProvider = jobProvider;
        }

        //
        // GET: /History/

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Trim()
        {
            _historyProvider.Trim();
            return Json(new NotificationResult() { Title = "Trimmed History items"});
        }

        public JsonResult Purge()
        {
            _historyProvider.Purge();
            return Json(new NotificationResult() { Title = "History Cleared" });
        }

        [HttpPost]
        public JsonResult Delete(int historyId)
        {
            //Delete the existing item from history
            _historyProvider.Delete(historyId);

            return Json(new NotificationResult() { Title = "History Item Deleted" });
        }

        [HttpPost]
        public JsonResult Redownload(int historyId, int episodeId)
        {
            //Delete the existing item from history
            _historyProvider.Delete(historyId);

            //Queue a job to download the replacement episode
            _jobProvider.QueueJob(typeof(EpisodeSearchJob), episodeId);

            return Json(new NotificationResult() { Title = "Episode Redownload Started" });
        }

        [GridAction]
        public ActionResult _AjaxBinding()
        {
            var history = _historyProvider.AllItemsWithRelationships().Select(h => new HistoryModel
                                            {
                                                HistoryId = h.HistoryId,
                                                SeriesId = h.SeriesId,
                                                SeasonNumber = h.Episode.SeasonNumber,
                                                EpisodeNumber = h.Episode.EpisodeNumber,
                                                EpisodeTitle = h.Episode.Title,
                                                EpisodeOverview = h.Episode.Overview,
                                                SeriesTitle = h.SeriesTitle,
                                                NzbTitle = h.NzbTitle,
                                                Quality = h.Quality.ToString(),
                                                IsProper = h.IsProper,
                                                Date = h.Date,
                                                Indexer = h.Indexer,
                                                EpisodeId = h.EpisodeId
                                            });

            return View(new GridModel(history));
        }
    }
}