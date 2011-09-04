using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class StreamController : Controller
    {
        //
        // GET: /Stream/

        public ActionResult Index()
        {
            return File(@"Z:\Clone High\Season 1\S01E02 - Episode Two- Election Blu-Galoo.avi", "video/divx");
            //return File(@"Z:\30 Rock\Season 5\S05E04 - Live Show (East Coast Taping) - HD TV.mkv", "video/divx");
        }
    }
}
