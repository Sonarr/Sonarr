using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using NzbDrone.Core.Jobs;
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

        public ActionResult Index()
        {
            var history = _historyProvider.AllItemsWithRelationships().Select(h => new HistoryModel
            {
                HistoryId = h.HistoryId,
                SeriesId = h.SeriesId,
                EpisodeNumbering = string.Format("{0}x{1:00}", h.Episode.SeasonNumber, h.Episode.EpisodeNumber),
                EpisodeTitle = h.Episode.Title,
                EpisodeOverview = h.Episode.Overview,
                SeriesTitle = h.SeriesTitle,
                NzbTitle = h.NzbTitle,
                Quality = h.Quality.ToString(),
                IsProper = h.IsProper,
                Date = h.Date.ToString(),
                Indexer = h.Indexer,
                EpisodeId = h.EpisodeId
            }).OrderByDescending(h => h.Date).ToList();

            var serialized = new JavaScriptSerializer().Serialize(history);

            return View((object)serialized);
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

            return JsonNotificationResult.Info("Episode Redownload Started");
        }
    }
}