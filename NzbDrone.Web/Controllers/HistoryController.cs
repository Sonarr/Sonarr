using System.Linq;
using System.Web.Mvc;
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
            
            var history = _historyProvider.AllItems().ToList().Select(h => new HistoryModel
                                                                      {
                                                                          HistoryId = h.HistoryId,
                                                                          SeasonNumber = h.Episode.SeasonNumber,
                                                                          EpisodeNumber = h.Episode.EpisodeNumber,
                                                                          EpisodeTitle = h.Episode.Title,
                                                                          EpisodeOverview = h.Episode.Overview,
                                                                          SeriesTitle = h.Episode.Series.Title,
                                                                          NzbTitle = h.NzbTitle,
                                                                          Quality = h.Quality.ToString(),
                                                                          IsProper = h.IsProper,
                                                                          Date = h.Date
                                                                      });

            history.ToList();

            return View(new GridModel(history));
        }
    }
}