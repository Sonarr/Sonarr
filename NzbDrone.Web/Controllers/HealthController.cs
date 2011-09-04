using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class HealthController : Controller
    {
        //
        // GET: /Health/

        [HttpGet]
        public JsonResult Index()
        {
            MvcMiniProfiler.MiniProfiler.Stop(true);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}
