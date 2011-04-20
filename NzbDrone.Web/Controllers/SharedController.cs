using System;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Timers;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        private readonly TimerProvider _timerProvider;

        public SharedController(TimerProvider timerProvider)
        {
            _timerProvider = timerProvider;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Series");
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            ViewData["RssTimer"] = DateTime.Now.ToString("yyyyMMddHHmmss");
            //ViewData["RssTimer"] = DateTime.Now.AddMinutes(61).AddSeconds(10).ToString("yyyyMMddHHmmss");
            return PartialView();
        }
    }
}