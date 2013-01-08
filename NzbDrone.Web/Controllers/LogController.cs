using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Web.Mvc;
using DataTables.Mvc.Core;
using DataTables.Mvc.Core.Models;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class LogController : Controller
    {
        private readonly LogProvider _logProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly DiskProvider _diskProvider;

        public LogController(LogProvider logProvider, EnvironmentProvider environmentProvider,
                                DiskProvider diskProvider)
        {
            _logProvider = logProvider;
            _environmentProvider = environmentProvider;
            _diskProvider = diskProvider;
        }

        public FileContentResult File()
        {
            string log = string.Empty;

            if (_diskProvider.FileExists(_environmentProvider.GetArchivedLogFileName()))
            {
                 log = _diskProvider.ReadAllText(_environmentProvider.GetArchivedLogFileName());
            } 
           
            log += _diskProvider.ReadAllText(_environmentProvider.GetLogFileName());

            return new FileContentResult(Encoding.ASCII.GetBytes(log), "text/plain");
        }

        public JsonResult Clear()
        {
            _logProvider.DeleteAll();

            return JsonNotificationResult.Info("Logs Cleared");
        }

        public ActionResult AjaxBinding(DataTablesPageRequest pageRequest)
        {
            var pageResult = _logProvider.GetPagedItems(pageRequest);
            var totalItems = _logProvider.Count();

            var items = pageResult.Items.Select(l => new LogModel
            {
                Time = l.Time.ToString(),
                Level = l.Level,
                Source = l.Logger,
                Message = l.Message,
                Method = l.Method,
                ExceptionType = l.ExceptionType,
                Exception = l.Exception
            });

            return Json(new
            {
                sEcho = pageRequest.Echo,
                iTotalRecords = totalItems,
                iTotalDisplayRecords = pageResult.TotalItems,
                aaData = items
            },
            JsonRequestBehavior.AllowGet);
        }
    }
}