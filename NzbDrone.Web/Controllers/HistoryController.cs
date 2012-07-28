using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DataTables.Mvc.Core.Models;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;

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

        public ActionResult Index()
        {
            return View();
        }

        //public JsonResult AjaxBinding()
        //{
        //    var history = _historyProvider.AllItemsWithRelationships().Select(h => new HistoryModel
        //    {
        //        HistoryId = h.HistoryId,
        //        SeriesId = h.SeriesId,
        //        EpisodeNumbering = string.Format("{0}x{1:00}", h.Episode.SeasonNumber, h.Episode.EpisodeNumber),
        //        EpisodeTitle = h.Episode.Title,
        //        EpisodeOverview = h.Episode.Overview,
        //        SeriesTitle = h.SeriesTitle,
        //        SeriesTitleSorter = SortHelper.SkipArticles(h.SeriesTitle),
        //        NzbTitle = h.NzbTitle,
        //        Quality = h.Quality.ToString(),
        //        IsProper = h.IsProper,
        //        Date = h.Date.ToString(),
        //        DateSorter = h.Date.ToString("MM/dd/yyyy h:mm:ss tt"),
        //        Indexer = h.Indexer,
        //        EpisodeId = h.EpisodeId,
        //        NzbInfoUrl = h.NzbInfoUrl
        //    }).OrderByDescending(h => h.Date).ToList();

        //    return Json(new
        //    {
        //        aaData = history
        //    },
        //    JsonRequestBehavior.AllowGet);
        //}

        public ActionResult AjaxBinding(DataTablesPageRequest pageRequest)
        {
            var pageResult = _historyProvider.GetPagedItems(pageRequest);
            var totalItems = _historyProvider.Count();

            var items = pageResult.Items.Select(h => new HistoryModel
            {
                HistoryId = h.HistoryId,
                SeriesId = h.SeriesId,
                EpisodeNumbering = string.Format("{0}x{1:00}", h.SeasonNumber, h.EpisodeNumber),
                EpisodeTitle = h.EpisodeTitle,
                EpisodeOverview = h.EpisodeOverview,
                SeriesTitle = h.SeriesTitle,
                SeriesTitleSorter = SortHelper.SkipArticles(h.SeriesTitle),
                NzbTitle = h.NzbTitle,
                Quality = h.Quality.ToString(),
                IsProper = h.IsProper,
                Date = h.Date.ToString(),
                DateSorter = h.Date.ToString("MM/dd/yyyy h:mm:ss tt"),
                Indexer = h.Indexer,
                EpisodeId = h.EpisodeId,
                NzbInfoUrl = h.NzbInfoUrl
            });

            return Json(new
            {
                sEcho = pageRequest.Echo,
                iTotalRecords = totalItems,
                iTotalDisplayRecords = pageResult.TotalItems,
                aaData = items
            },
            JsonRequestBehavior.AllowGet);
        }

        public JsonResult Trim()
        {
            _historyProvider.Trim();
            return JsonNotificationResult.Info("Trimmed History");
        }

        public JsonResult Purge()
        {
            _historyProvider.Purge();
            return JsonNotificationResult.Info("History Cleared");
        }

        [HttpPost]
        public JsonResult Delete(int historyId)
        {
            //Delete the existing item from history
            _historyProvider.Delete(historyId);

            return JsonNotificationResult.Info("History Item Deleted");
        }

        [HttpPost]
        public JsonResult Redownload(int historyId, int episodeId)
        {
            //Delete the existing item from history
            _historyProvider.Delete(historyId);

            //Queue a job to download the replacement episode
            _jobProvider.QueueJob(typeof(EpisodeSearchJob), episodeId);

            return JsonNotificationResult.Queued("Episode search");
        }
    }
}