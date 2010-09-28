using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SettingsController : Controller
    {
        //
        // GET: /Settings/
        private IConfigProvider _configProvider;

        public SettingsController(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public ActionResult Index()
        {
            return View(new SettingsModel() { TvFolder = _configProvider.SeriesRoot });
        }

        [HttpPost]
        public ActionResult Index(SettingsModel model)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SeriesRoot = model.TvFolder;
                //return RedirectToAction("index");
            }

            return View(model);
        }

    }
}
