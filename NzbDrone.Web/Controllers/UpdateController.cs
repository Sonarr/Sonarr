using System;
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
        private readonly EnvironmentProvider _environmentProvider;
        private readonly DiskProvider _diskProvider;

        public UpdateController(UpdateProvider updateProvider, JobProvider jobProvider,
            EnvironmentProvider environmentProvider, DiskProvider diskProvider)
        {
            _updateProvider = updateProvider;
            _jobProvider = jobProvider;
            _environmentProvider = environmentProvider;
            _diskProvider = diskProvider;
        }

        public ActionResult Index()
        {
            var updateModel = new UpdateModel();
            updateModel.UpdatePackage = _updateProvider.GetAvilableUpdate(_environmentProvider.Version);
            updateModel.LogFiles = _updateProvider.UpdateLogFile();
            updateModel.LogFolder = _environmentProvider.GetUpdateLogFolder();

            return View(updateModel);
        }

        public ActionResult StartUpdate()
        {
            _jobProvider.QueueJob(typeof(AppUpdateJob));

            return JsonNotificationResult.Info("Update will begin shortly", "NzbDrone will restart automatically.");
        }

        public ActionResult ViewLog( string filepath)
        {
            ViewBag.Log = _diskProvider.ReadAllText(filepath).Replace(Environment.NewLine, "<br/>");
            return View();
        }

        [HttpGet]
        public ActionResult Post(string expectedVersion)
        {
            var model = new PostUpgradeModel();
            model.CurrentVersion = _environmentProvider.Version;
            model.ExpectedVersion = Version.Parse(expectedVersion);
            model.Success = model.CurrentVersion >= model.ExpectedVersion;
            
            if (!model.Success)
                model.LogFile = _updateProvider.UpdateLogFile().FirstOrDefault();

            return View(model);
        }
    }
}
