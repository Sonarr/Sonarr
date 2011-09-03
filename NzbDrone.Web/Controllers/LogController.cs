using System.Web.Mvc;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Web.Models;
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

        public JsonResult Clear()
        {
            _logProvider.DeleteAll();

            return Json(new NotificationResult() { Title = "Logs Cleared" });
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _AjaxBinding(GridCommand gridCommand)
        {
            var logs = _logProvider.GetPagedLogs(gridCommand.Page, gridCommand.PageSize);

            return View(new GridModel{ Data = logs.Items, Total = (int)logs.TotalItems });
        }
    }
}