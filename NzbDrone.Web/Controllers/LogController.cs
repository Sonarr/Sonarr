using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Instrumentation;
using SubSonic.Repository;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class LogController : Controller
    {
        private readonly ILogProvider _logProvider;

        public LogController(ILogProvider logProvider)
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


        [GridAction]
        public ActionResult _AjaxBinding2()
        {
           var l= _logProvider.GetAllLogs().Select(c => new {
                                                          c.DisplayLevel,
                                                          c.ExceptionMessage,
                                                          c.ExceptionString,
                                                          c.ExceptionType,
                                                          //c.Level,
                                                          c.Logger,
                                                          c.LogId,
                                                          c.Message,
                                                          c.Stack,
                                                          c.Time
                                                      });

           return View(new GridModel(l));
        }



    }
}
