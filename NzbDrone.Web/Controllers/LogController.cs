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
            var logs = _logProvider.GetAllLogs();
            var totalCount = logs.Count();

            IQueryable<Log> q = logs;
            if (!string.IsNullOrEmpty(pageRequest.Search))
            {
                q = q.Where(b => b.Logger.Contains(pageRequest.Search)
                    || b.Exception.Contains(pageRequest.Search)
                    || b.Message.Contains(pageRequest.Search));
            }

            int filteredCount = q.Count();

            IQueryable<Log> sorted = q;

            for (int i = 0; i < pageRequest.SortingCols; i++)
            {
                int sortCol = pageRequest.SortCol[i];
                var sortColName = sortCol == 0 ? "Time" : sortCol == 1 ? "Level" : "Logger";
                var sortExpression = String.Format("{0} {1}", sortColName, pageRequest.SortDir[i]);

                sorted = sorted.OrderBy(sortExpression);
            }

            IQueryable<Log> filteredAndSorted = sorted;
            if (filteredCount > pageRequest.DisplayLength)
            {
                filteredAndSorted = sorted.Skip(pageRequest.DisplayStart).Take(pageRequest.DisplayLength);
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
                sEcho = pageRequest.Echo,
                iTotalRecords = totalCount,
                iTotalDisplayRecords = filteredCount,
                aaData = logModels
            },
            JsonRequestBehavior.AllowGet);
        }
    }
}