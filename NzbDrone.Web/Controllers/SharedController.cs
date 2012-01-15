using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        private readonly EnviromentProvider _enviromentProvider;

        public SharedController(EnviromentProvider enviromentProvider)
        {
            _enviromentProvider = enviromentProvider;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Series");
        }

        [ChildActionOnly]
        [OutputCache(Duration = 3600)]
        public ActionResult Footer()
        {
            return PartialView(new FooterModel { BuildTime = _enviromentProvider.BuildDateTime, Version = _enviromentProvider.Version });
        }
    }
}