using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
using Telerik.Web.Mvc;

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
            return View(_indexerProvider.GetAllISettings());
        }


        public ActionResult Config()
        {
            return View(_configProvider.All());
        }


        [GridAction]
        public ActionResult _SelectAjaxEditing()
        {
            return View(new GridModel(_configProvider.All()));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveAjaxEditing(string key, string value)
        {
            _configProvider.SetValue(key, value);
            return View(new GridModel(_configProvider.All()));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _InsertAjaxEditing(string key, string value)
        {

            _configProvider.SetValue(key, value);
            return View(new GridModel(_configProvider.All()));
        }
    }
}
