using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Web.Controllers
{
    public class JobController : Controller
    {
        private readonly JobProvider _jobProvider;

        public JobController(JobProvider jobProvider)
        {
            _jobProvider = jobProvider;
        }

        public ActionResult Index()
        {
            return View(_jobProvider.All());
        }


    }
}
