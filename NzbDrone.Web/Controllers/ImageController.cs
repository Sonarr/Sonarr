using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Common;

namespace NzbDrone.Web.Controllers
{
    public class ImageController : Controller
    {
        private readonly DiskProvider _diskProvider;

        public ImageController(DiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public ActionResult Newznab(string name)
        {
            var dir = Server.MapPath("/Content/Images/Indexers");
            var path = Path.Combine(dir, String.Format("{0}.png", name));

            if (_diskProvider.FileExists(path))
                return File(path, "image/png");

            return File(Path.Combine(dir, "Newznab.png"), "image/png");
        }
    }
}
