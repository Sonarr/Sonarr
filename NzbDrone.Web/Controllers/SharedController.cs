using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.RootFolders;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        private readonly EnvironmentProvider _environmentProvider;
        private readonly RootFolderService _rootFolderService;

        public SharedController(EnvironmentProvider environmentProvider, RootFolderService rootFolderService)
        {
            _environmentProvider = environmentProvider;
            _rootFolderService = rootFolderService;
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

        [ChildActionOnly]
        [OutputCache(Duration = 600)]
        public ActionResult FreeSpace()
        {
            var rootDirs = _rootFolderService.FreeSpaceOnDrives();

            return PartialView(rootDirs);
        }
    }
}