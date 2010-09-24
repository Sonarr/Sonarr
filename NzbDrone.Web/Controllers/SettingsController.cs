using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Controllers;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SettingsController : Controller
    {
        //
        // GET: /Settings/
        private IConfigController _configController;

        public SettingsController(IConfigController configController)
        {
            _configController = configController;
        }

        public ActionResult Index()
        {
            return View(new SettingsModel() { TvFolder = _configController.SeriesRoot });
        }

        [HttpPost]
        public ActionResult Index(SettingsModel model)
        {
            if (ModelState.IsValid)
            {
                _configController.SeriesRoot = model.TvFolder;
                //return RedirectToAction("index");
            }

            return View(model);
        }

    }
}
