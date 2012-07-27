using System.Web.Mvc;
using StackExchange.Profiling;

namespace NzbDrone.Web.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet]
        public JsonResult Index()
        {
            MiniProfiler.Stop(true);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}
