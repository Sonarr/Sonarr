using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class UpdateController : Controller
    {
        private readonly UpdateProvider _updateProvider;
        private readonly JobProvider _jobProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly DiskProvider _diskProvider;

        public UpdateController(UpdateProvider updateProvider, JobProvider jobProvider,
            EnviromentProvider enviromentProvider, DiskProvider diskProvider)
        {
            _updateProvider = updateProvider;
            _jobProvider = jobProvider;
            _enviromentProvider = enviromentProvider;
            _diskProvider = diskProvider;
        }

        public ActionResult Index()
        {
            var updateModel = new UpdateModel();
            updateModel.UpdatePackage = _updateProvider.GetAvilableUpdate();
            updateModel.LogFiles = _updateProvider.UpdateLogFile();
            updateModel.LogFolder = _enviromentProvider.GetUpdateLogFolder();

            return View(updateModel);
        }

        public ActionResult StartUpdate()
        {
            _jobProvider.QueueJob(typeof(AppUpdateJob), 0, 0);

            return Json(new NotificationResult { Title = "Update will begin shortly", NotificationType = NotificationType.Info, Text = "NzbDrone will restart automatically." });
        }

        public ActionResult ViewLog( string filepath)
        {
            ViewBag.Log = _diskProvider.ReadAllText(filepath).Replace(Environment.NewLine, "<br/>");
            return View();
        }
    }
}
