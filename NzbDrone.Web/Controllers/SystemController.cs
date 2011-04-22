using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Web.Controllers
{
    public class SystemController : Controller
    {
        private readonly JobProvider _jobProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly ConfigProvider _configProvider;

        public SystemController(JobProvider jobProvider, IndexerProvider indexerProvider, ConfigProvider configProvider)
        {
            _jobProvider = jobProvider;
            _indexerProvider = indexerProvider;
            _configProvider = configProvider;
        }

        public ActionResult Jobs()
        {
            return View(_jobProvider.All());
        }

        public ActionResult Indexers()
        {
            return View(_indexerProvider.All());
        }


        public ActionResult Config()
        {
            return View(_configProvider.All());
        }


    }
}
