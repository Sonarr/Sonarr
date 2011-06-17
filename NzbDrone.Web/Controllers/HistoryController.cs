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

            //TODO: possible subsonic bug, IQuarible causes some issues so ToList() is called
            //https://github.com/subsonic/SubSonic-3.0/issues/263

            var historyDb = _historyProvider.AllItems().ToList();
            
            var history = new List<HistoryModel>();
            
            foreach (var item in historyDb)
            {
                var episode = item.Episode;
                var series = episode.Series;

                history.Add(new HistoryModel
                                            {
                                                HistoryId = item.HistoryId,
                                                SeasonNumber = episode.SeasonNumber,
                                                EpisodeNumber = episode.EpisodeNumber,
                                                EpisodeTitle = episode.Title,
                                                EpisodeOverview = episode.Overview,
                                                SeriesTitle = series.Title,
                                                NzbTitle = item.NzbTitle,
                                                Quality = item.Quality.ToString(),
                                                IsProper = item.IsProper,
                                                Date = item.Date,
                                                Indexer = item.Indexer
                                            });
            }

            return View(new GridModel(history));
        }
    }
}