using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public ActionResult Test()
        {
            return Content("Testing...");
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

        public SelectList GetDirectories(string text)
        {
            //Windows (Including UNC)
            var windowsSep = text.LastIndexOf('\\');

            if (windowsSep > -1)
            {
                var dirs = _diskProvider.GetDirectories(text.Substring(0, windowsSep + 1));
                return new SelectList(dirs, dirs.FirstOrDefault());
            }

            //Unix
            var index = text.LastIndexOf('/');

            if (index > -1)
            {
                var dirs = _diskProvider.GetDirectories(text.Substring(0, index + 1));
                return new SelectList(dirs, dirs.FirstOrDefault());
            }

            return new SelectList(new List<string>());
        }
    }
}
