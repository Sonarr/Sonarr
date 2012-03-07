using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        private readonly EnvironmentProvider _environmentProvider;

        public SharedController(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Series");
        }

        [ChildActionOnly]
        [OutputCache(Duration = 3600)]
        public ActionResult Footer()
        {
            return PartialView(new FooterModel { BuildTime = _environmentProvider.BuildDateTime, Version = _environmentProvider.Version });
        }
    }
}