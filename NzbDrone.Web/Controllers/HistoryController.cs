using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class HistoryController : Controller
    {
        private readonly HistoryProvider _historyProvider;

        public HistoryController(HistoryProvider historyProvider)
        {
            _historyProvider = historyProvider;
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
            return new JsonResult { Data = "ok" };
        }

        public JsonResult Purge()
        {
            _historyProvider.Purge();
            return new JsonResult { Data = "ok" };
        }

        [GridAction]
        public ActionResult _AjaxBinding()
        {
            var history = _historyProvider.AllItemsWithRelationships().Select(h => new HistoryModel
                                            {
                                                HistoryId = h.HistoryId,
                                                SeasonNumber = h.Episode.SeasonNumber,
                                                EpisodeNumber = h.Episode.EpisodeNumber,
                                                EpisodeTitle = h.Episode.Title,
                                                EpisodeOverview = h.Episode.Overview,
                                                SeriesTitle = h.SeriesTitle,
                                                NzbTitle = h.NzbTitle,
                                                Quality = h.Quality.ToString(),
                                                IsProper = h.IsProper,
                                                Date = h.Date,
                                                Indexer = h.Indexer
                                            });

            return View(new GridModel(history));
        }
    }
}