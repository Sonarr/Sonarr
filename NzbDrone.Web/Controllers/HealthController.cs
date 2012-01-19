using System.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet]
        public JsonResult Index()
        {
            MvcMiniProfiler.MiniProfiler.Stop(true);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}
