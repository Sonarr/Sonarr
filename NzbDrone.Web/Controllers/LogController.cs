using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
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

        public ActionResult AjaxBinding(DataTablesParams dataTablesParams)
        {
            var logs = _logProvider.GetAllLogs();
            var totalCount = logs.Count();

            IQueryable<Log> q = logs;
            if (!string.IsNullOrEmpty(dataTablesParams.sSearch))
            {
                q = q.Where(b => b.Logger.Contains(dataTablesParams.sSearch)
                    || b.Exception.Contains(dataTablesParams.sSearch)
                    || b.Message.Contains(dataTablesParams.sSearch));
            }

            int filteredCount = q.Count();

            int sortCol = dataTablesParams.iSortCol.First();
            var sortColName = sortCol == 0 ? "Time" : sortCol == 1 ? "Level" : "Logger";
            var sortExpression = String.Format("{0} {1}", sortColName, dataTablesParams.sSortDir.First());

            var sorted = q.OrderBy(sortExpression);

            IQueryable<Log> filteredAndSorted = sorted;
            if (filteredCount > dataTablesParams.iDisplayLength)
            {
                filteredAndSorted = sorted.Skip(dataTablesParams.iDisplayStart).Take(dataTablesParams.iDisplayLength);
            }

            var logModels = filteredAndSorted.ToList().Select(s => new LogModel
                                                 {
                                                         Time = s.Time.ToString(),
                                                         Level = s.Level,
                                                         Source = s.Logger,
                                                         Message = s.Message,
                                                         Method = s.Method,
                                                         ExceptionType = s.ExceptionType,
                                                         Exception = s.Exception
                                                 });

            return Json(new
            {
                sEcho = dataTablesParams.sEcho,
                iTotalRecords = totalCount,
                iTotalDisplayRecords = filteredCount,
                aaData = logModels
            },
            JsonRequestBehavior.AllowGet);
        }
    }
}