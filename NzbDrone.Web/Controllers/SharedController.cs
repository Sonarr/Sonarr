using System;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        private readonly JobProvider _jobProvider;

        public SharedController(JobProvider jobProvider)
        {
            _jobProvider = jobProvider;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Series");
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            ViewData["RssTimer"] = _jobProvider.NextScheduledRun(typeof(RssSyncJob)).ToString("yyyyMMddHHmmss");
            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult LocalSearch()
        {
            return PartialView();
        }
    }
}