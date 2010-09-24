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
            return View(new SettingsModel() { RootPath = _configController.SeriesRoot });
        }

        [HttpPost]
        public ActionResult Save(SettingsModel model)
        {
            if (ModelState.IsValid)
            {
                _configController.SeriesRoot = model.RootPath;
            }
            
            return RedirectToAction("index");
        }

    }
}
