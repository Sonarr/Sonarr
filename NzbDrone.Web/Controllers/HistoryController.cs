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
        private readonly EpisodeProvider _episodeProvider;
        private readonly SeriesProvider _seriesProvider;

        public HistoryController(HistoryProvider historyProvider, EpisodeProvider episodeProvider,
            SeriesProvider seriesProvider)
        {
            _historyProvider = historyProvider;
            _episodeProvider = episodeProvider;
            _seriesProvider = seriesProvider;
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
            var historyDb = _historyProvider.AllItems();
            
            var history = new List<HistoryModel>();

            foreach (var item in historyDb)
            {
                var episode = _episodeProvider.GetEpisode(item.EpisodeId);
                var series = _seriesProvider.GetSeries(item.SeriesId);
       
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