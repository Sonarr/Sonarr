using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class HistoryController : Controller
    {
        private HistoryProvider _historyProvider;

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

        public ActionResult Trim()
        {
            _historyProvider.Trim();
            return RedirectToAction("Index");
        }

        public ActionResult Purge()
        {
            _historyProvider.Purge();
            return RedirectToAction("Index");
        }

        [GridAction]
        public ActionResult _AjaxBinding()
        {
            var history = _historyProvider.AllItems().Select(h => new HistoryModel
                                                                      {
                                                                          HistoryId = h.HistoryId,
                                                                          SeasonNumber = h.Episode.SeasonNumber,
                                                                          EpisodeNumber = h.Episode.EpisodeNumber,
                                                                          EpisodeTitle = h.Episode.Title,
                                                                          EpisodeOverview = h.Episode.Overview,
                                                                          SeriesTitle = h.Episode.Series.Title,
                                                                          NzbTitle = h.NzbTitle,
                                                                          Quality = h.Quality.ToString("G"),
                                                                          IsProper = h.IsProper,
                                                                          Date = h.Date
                                                                      });

            return View(new GridModel(history));
        }
    }
}
