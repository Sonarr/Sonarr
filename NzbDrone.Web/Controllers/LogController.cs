using System.Web.Mvc;
using NzbDrone.Core.Instrumentation;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class LogController : Controller
    {
        private readonly LogProvider _logProvider;

        public LogController(LogProvider logProvider)
        {
            _logProvider = logProvider;
        }

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Clear()
        {
            _logProvider.DeleteAll();
            return RedirectToAction("Index");
        }

        [GridAction]
        public ActionResult _AjaxBinding()
        {
            return View(new GridModel(_logProvider.GetAllLogs()));
        }
    }
}