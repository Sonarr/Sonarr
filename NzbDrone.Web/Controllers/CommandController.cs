using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Web.Controllers
{
    public class CommandController : Controller
    {
        private readonly JobProvider _jobProvider;

        public CommandController(JobProvider jobProvider)
        {
            _jobProvider = jobProvider;
        }

        public JsonResult RssSync()
        {
            _jobProvider.QueueJob(typeof(RssSyncJob));
            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult SyncEpisodesOnDisk(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(DiskScanJob), seriesId);

            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult UpdateInfo(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(UpdateInfoJob), seriesId);

            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
