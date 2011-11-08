using System;
using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        private readonly JobProvider _jobProvider;
        private readonly EnviromentProvider _enviromentProvider;

        public SharedController(JobProvider jobProvider, EnviromentProvider enviromentProvider)
        {
            _jobProvider = jobProvider;
            _enviromentProvider = enviromentProvider;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Series");
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {

            return PartialView(new FooterModel { BuildTime = _enviromentProvider.BuildDateTime, Version = _enviromentProvider.Version });
        }

        [ChildActionOnly]
        public ActionResult LocalSearch()
        {
            return PartialView();
        }
    }
}