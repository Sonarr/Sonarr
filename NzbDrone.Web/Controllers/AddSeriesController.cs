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
    public class AddSeriesController : Controller
    {
        private readonly ConfigProvider _configProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly RootDirProvider _rootFolderProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly JobProvider _jobProvider;
        private readonly SyncProvider _syncProvider;
        private readonly TvDbProvider _tvDbProvider;

        public AddSeriesController(SyncProvider syncProvider, RootDirProvider rootFolderProvider,
                                   ConfigProvider configProvider,
                                   QualityProvider qualityProvider, TvDbProvider tvDbProvider,
                                   SeriesProvider seriesProvider, JobProvider jobProvider)
        {
            _syncProvider = syncProvider;
            _rootFolderProvider = rootFolderProvider;
            _configProvider = configProvider;
            _qualityProvider = qualityProvider;
            _tvDbProvider = tvDbProvider;
            _seriesProvider = seriesProvider;
            _jobProvider = jobProvider;
        }

        [HttpPost]
        public JsonResult ScanNewSeries()
        {
            _jobProvider.BeginExecute(typeof(NewSeriesUpdate));
            return new JsonResult();
        }

        public ActionResult AddNew()
        {
            ViewData["RootDirs"] = _rootFolderProvider.GetAll();
            ViewData["DirSep"] = Path.DirectorySeparatorChar;

            var profiles = _qualityProvider.GetAllProfiles();
            var selectList = new SelectList(profiles, "QualityProfileId", "Name");
            var defaultQuality = Convert.ToInt32(_configProvider.DefaultQualityProfile);

            var model = new AddNewSeriesModel
                            {
                                DirectorySeparatorChar = Path.DirectorySeparatorChar.ToString(),
                                RootDirectories = _rootFolderProvider.GetAll(),
                                QualityProfileId = defaultQuality,
                                QualitySelectList = selectList
                            };

            return View(model);
        }

        public ActionResult AddExisting()
        {
            var unmappedList = new List<String>();

            var profiles = _qualityProvider.GetAllProfiles();
            var defaultQuality = Convert.ToInt32(_configProvider.DefaultQualityProfile);
            var selectList = new SelectList(profiles, "QualityProfileId", "Name", defaultQuality);

            ViewData["qualities"] = selectList;

            foreach (var folder in _rootFolderProvider.GetAll())
            {
                unmappedList.AddRange(_syncProvider.GetUnmappedFolders(folder.Path));
            }

            return View(unmappedList);
        }

        public ActionResult RenderPartial(string path)
        {
            var suggestions = GetSuggestionList(new DirectoryInfo(path).Name);

            ViewData["guid"] = Guid.NewGuid();
            ViewData["path"] = path;
            ViewData["javaPath"] = path.Replace(Path.DirectorySeparatorChar, '|').Replace(Path.VolumeSeparatorChar, '^');

            var defaultQuality = _configProvider.DefaultQualityProfile;
            var qualityProfiles = _qualityProvider.GetAllProfiles();

            ViewData["quality"] = new SelectList(
                qualityProfiles,
                "QualityProfileId",
                "Name",
                defaultQuality);

            return PartialView("AddSeriesItem", suggestions);
        }

        public JsonResult AddSeries(string path, int seriesId, int qualityProfileId)
        {
            //Get TVDB Series Name
            //Create new folder for series
            //Add the new series to the Database

            _seriesProvider.AddSeries(
                path.Replace('|', Path.DirectorySeparatorChar).Replace('^', Path.VolumeSeparatorChar), seriesId,
                qualityProfileId);
            ScanNewSeries();
            return new JsonResult { Data = "ok" };
        }

        [HttpPost]
        public ActionResult _textLookUp(string text, int? filterMode)
        {
            var suggestions = GetSuggestionList(text);

            return new JsonResult
                       {
                           JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                           Data = suggestions
                       };
        }

        public SelectList GetSuggestionList(string searchString)
        {
            var dataVal = _tvDbProvider.SearchSeries(searchString);

            int selectId = 0;
            if (dataVal.Count != 0)
            {
                selectId = dataVal[0].Id;
            }

            return new SelectList(dataVal, "Id", "SeriesName", selectId);
        }
    }
}