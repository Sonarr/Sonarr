using System;
using System.Text;
using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class LogController : Controller
    {
        private readonly LogProvider _logProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly DiskProvider _diskProvider;

        public LogController(LogProvider logProvider, EnviromentProvider enviromentProvider, DiskProvider diskProvider)
        {
            _logProvider = logProvider;
            _enviromentProvider = enviromentProvider;
            _diskProvider = diskProvider;
        }

        public ActionResult Index()
        {
            return View();
        }

        public FileContentResult File()
        {
            string log = string.Empty;

            if (_diskProvider.FileExists(_enviromentProvider.GetArchivedLogFileName()))
            {
                 log = _diskProvider.ReadAllText(_enviromentProvider.GetArchivedLogFileName());
            } 
           
            log += _diskProvider.ReadAllText(_enviromentProvider.GetLogFileName());

            return new FileContentResult(Encoding.ASCII.GetBytes(log), "text/plain");
        }

        public JsonResult Clear()
        {
            _logProvider.DeleteAll();

            return JsonNotificationResult.Info("Logs Cleared");
        }

        [GridAction]
        public ActionResult AjaxBinding()
        {
            return View(new GridModel(_logProvider.GetAllLogs()));
        }
    }
}