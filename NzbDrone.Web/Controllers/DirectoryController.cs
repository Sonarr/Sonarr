using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Web.Controllers
{
    public class DirectoryController : Controller
    {
        private readonly DiskProvider _diskProvider;

        public DirectoryController(DiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        [HttpPost]
        public ActionResult _autoCompletePath(string text, int? filterMode)
        {
            var data = GetDirectories(text);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = data
            };
        }

        [HttpGet]
        public JsonResult GetDirectories(string q)
        {
            try
            {
                //Windows (Including UNC)
                var windowsSep = q.LastIndexOf('\\');

                if (windowsSep > -1)
                {
                    var dirs = _diskProvider.GetDirectories(q.Substring(0, windowsSep + 1));
                    return Json(dirs, JsonRequestBehavior.AllowGet);
                }

                return Json(new string[] { }, JsonRequestBehavior.AllowGet);
                //Unix
                var index = q.LastIndexOf('/');

                if (index > -1)
                {
                    var dirs = _diskProvider.GetDirectories(q.Substring(0, index + 1));
                    //return new SelectList(dirs, dirs.FirstOrDefault());
                }
            }
            catch
            {
                //Swallow the exceptions so proper JSON is returned to the client (Empty results)
            }

            return Json(new string[]{}, JsonRequestBehavior.AllowGet);
        }
    }
}
