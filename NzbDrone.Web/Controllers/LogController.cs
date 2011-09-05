using System.Web.Mvc;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;
using System.Linq;

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

        public ActionResult All()
        {
            return View();
        }

        public JsonResult Clear()
        {
            _logProvider.DeleteAll();

            return Json(new NotificationResult { Title = "Logs Cleared" });
        }

        [GridAction]
        public ActionResult _TopAjaxBinding()
        {
            var logs = _logProvider.TopLogs();

            return View(new GridModel(logs));
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AllAjaxBinding(GridCommand gridCommand)
        {
            var logs = _logProvider.GetPagedLogs(gridCommand.Page, gridCommand.PageSize);

            return View(new GridModel{ Data = logs.Items, Total = (int)logs.TotalItems });
        }
    }
}