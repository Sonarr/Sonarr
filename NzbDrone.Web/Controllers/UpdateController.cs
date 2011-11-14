using System.Linq;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class UpdateController : Controller
    {
        private readonly UpdateProvider _updateProvider;
        private readonly JobProvider _jobProvider;

        public UpdateController(UpdateProvider updateProvider, JobProvider jobProvider)
        {
            _updateProvider = updateProvider;
            _jobProvider = jobProvider;
        }

        public ActionResult Index()
        {
            return View(_updateProvider.GetAvilableUpdate());
        }

        public ActionResult StartUpdate()
        {
            _jobProvider.QueueJob(typeof(AppUpdateJob), 0, 0);

            return Json(new NotificationResult() { Title = "Update will begin shortly", NotificationType = NotificationType.Info, Text = "NzbDrone will restart automatically."});
        }
    }
}
