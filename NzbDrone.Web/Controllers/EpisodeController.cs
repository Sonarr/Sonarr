using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class EpisodeController : Controller
    {

        private readonly JobProvider _jobProvider;


        public EpisodeController(JobProvider jobProvider)
        {

            _jobProvider = jobProvider;
        }

        public JsonResult Search(int episodeId)
        {
            _jobProvider.QueueJob(typeof(EpisodeSearchJob), episodeId);
            return new JsonResult { Data = "ok" };
        }


    }
}