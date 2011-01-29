using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class SharedController : Controller
    {
        //
        // GET: /Shared/

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Series");
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            ViewData["RssTimer"] = DateTime.Now.AddMinutes(61).AddSeconds(10).ToString("yyyyMMddHHmmss");
            return PartialView();
        }
    }
}
