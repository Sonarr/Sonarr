using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers.Timers;

namespace NzbDrone.Web.Controllers
{
    public class TimersController : Controller
    {
        private readonly TimerProvider _timerProvider;

        public TimersController(TimerProvider timerProvider)
        {
            _timerProvider = timerProvider;
        }

        public ActionResult Index()
        {
            return View(_timerProvider.All());
        }


    }
}
