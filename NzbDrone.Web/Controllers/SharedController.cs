using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        private ITimerProvider _timerProvider;

        public SharedController(ITimerProvider timerProvider)
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
            ViewData["RssTimer"] = _timerProvider.NextRssSyncTime().ToString("yyyyMMddHHmmss");
            //ViewData["RssTimer"] = DateTime.Now.AddMinutes(61).AddSeconds(10).ToString("yyyyMMddHHmmss");
            return PartialView();
        }
    }
}
