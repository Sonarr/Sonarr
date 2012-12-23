using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DataTables.Mvc.Core.Models;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class HistoryController : Controller
    {
        private readonly HistoryProvider _historyProvider;
        private readonly JobProvider _jobProvider;
        private readonly ConfigProvider _configProvider;

        public HistoryController(HistoryProvider historyProvider, JobProvider jobProvider,
                                    ConfigProvider configProvider)
        {
            _historyProvider = historyProvider;
            _jobProvider = jobProvider;
            _configProvider = configProvider;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AjaxBinding(DataTablesPageRequest pageRequest)
        {
            var pageResult = _historyProvider.GetPagedItems(pageRequest);
            var totalItems = _historyProvider.Count();
            var ignoreArticles = _configProvider.IgnoreArticlesWhenSortingSeries;

            var items = pageResult.Items.Select(h => new HistoryModel
            {
                HistoryId = h.HistoryId,
                SeriesId = h.SeriesId,
                EpisodeNumbering = string.Format("{0}x{1:00}", h.SeasonNumber, h.EpisodeNumber),
                EpisodeTitle = h.EpisodeTitle,
                EpisodeOverview = h.EpisodeOverview,
                SeriesTitle = h.SeriesTitle,
                SeriesTitleSorter = ignoreArticles ? h.SeriesTitle.IgnoreArticles() : h.SeriesTitle,
                NzbTitle = h.NzbTitle,
                Quality = h.Quality.ToString(),
                IsProper = h.IsProper,
                Date = h.Date.ToString(),
                DateSorter = h.Date.ToString("o", CultureInfo.InvariantCulture),
                Indexer = h.Indexer,
                EpisodeId = h.EpisodeId,
                NzbInfoUrl = h.NzbInfoUrl,
                ReleaseGroup = h.ReleaseGroup
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
            _jobProvider.QueueJob(typeof(EpisodeSearchJob), new { EpisodeId = episodeId });

            return JsonNotificationResult.Queued("Episode search");
        }
    }
}