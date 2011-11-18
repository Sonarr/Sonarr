using System.Collections.Generic;
using System.Web.Mvc;
using NzbDrone.Common;

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
        public JsonResult GetDirectories(string term)
        {
            IEnumerable<string> dirs = null;
            try
            {
                //Windows (Including UNC)
                var windowsSep = term.LastIndexOf('\\');

                if (windowsSep > -1)
                {
                    dirs = _diskProvider.GetDirectories(term.Substring(0, windowsSep + 1));

                }

                //Unix
                var index = term.LastIndexOf('/');

                if (index > -1)
                {
                    dirs = _diskProvider.GetDirectories(term.Substring(0, index + 1));
                }
            }
            catch
            {
                //Swallow the exceptions so proper JSON is returned to the client (Empty results)
            }

            return Json(dirs, JsonRequestBehavior.AllowGet);
        }
    }
}
