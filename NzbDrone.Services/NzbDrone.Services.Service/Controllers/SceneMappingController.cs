using System.Linq;
using System.Web.Mvc;
using NzbDrone.Services.Service.Providers;

namespace NzbDrone.Services.Service.Controllers
{
    public class SceneMappingController : Controller
    {
        private readonly SceneMappingProvider _sceneMappingProvider;

        public SceneMappingController(SceneMappingProvider sceneMappingProvider)
        {
            _sceneMappingProvider = sceneMappingProvider;
        }

        [HttpGet]
        [OutputCache(CacheProfile = "Cache1Hour")]
        public JsonResult Active()
        {
            var mappings = _sceneMappingProvider.AllLive();

            return Json(mappings, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Pending()
        {
            var mappings = _sceneMappingProvider.AllPending();

            return Json(mappings, JsonRequestBehavior.AllowGet);
        }
    }
}