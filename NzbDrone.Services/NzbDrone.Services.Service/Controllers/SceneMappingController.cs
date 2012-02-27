using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using NzbDrone.Services.Service.Providers;
using NzbDrone.Services.Service.Repository;

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
        [Authorize(Roles = "Users")]
        public ActionResult Pending()
        {
            var mappings = _sceneMappingProvider.AllPending();
            var serialized = JsonConvert.SerializeObject(mappings);

            return View((object)serialized);
        }

        [HttpPost]
        [Authorize(Roles = "Users")]
        public string UpdatePending(int id, string value, int columnId)
        {
            var mapping = _sceneMappingProvider.GetPending(id);

            if (columnId == 0)
                mapping.CleanTitle = value.Trim();

            if (columnId == 1)
                mapping.Id = Int32.Parse(value);

            if (columnId == 2)
                mapping.Title = value.Trim();

            _sceneMappingProvider.Update(mapping);

            return value;
        }

        [HttpPost]
        [Authorize(Roles = "Users")]
        public JsonResult AddPending(string cleanTitle, int id, string title)
        {
            _sceneMappingProvider.Insert(new PendingSceneMapping { CleanTitle = cleanTitle, Id = id, Title = title });

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Users")]
        public JsonResult Promote(int mappingId)
        {
            _sceneMappingProvider.Promote(mappingId);
            HttpResponse.RemoveOutputCacheItem(VirtualPathUtility.ToAbsolute("~/SceneMapping/Active"));

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Users")]
        public JsonResult PromoteAll()
        {
            _sceneMappingProvider.PromoteAll();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Users")]
        public JsonResult Delete(int mappingId)
        {
            _sceneMappingProvider.DeletePending(mappingId);
            HttpResponse.RemoveOutputCacheItem(VirtualPathUtility.ToAbsolute("~/SceneMapping/Active"));

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }
    }
}