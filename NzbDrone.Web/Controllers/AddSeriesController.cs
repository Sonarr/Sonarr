using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class AddSeriesController : Controller
    {
        public IConfigProvider ConfigProvider { get; set; }
        private readonly ISyncProvider _syncProvider;
        private readonly IRootDirProvider _rootFolderProvider;
        private readonly IConfigProvider _configProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly ISeriesProvider _seriesProvider;

        public AddSeriesController(ISyncProvider syncProvider, IRootDirProvider rootFolderProvider, IConfigProvider configProvider,
            QualityProvider qualityProvider, TvDbProvider tvDbProvider, ISeriesProvider seriesProvider)
        {
            ConfigProvider = configProvider;
            _syncProvider = syncProvider;
            _rootFolderProvider = rootFolderProvider;
            _configProvider = configProvider;
            _qualityProvider = qualityProvider;
            _tvDbProvider = tvDbProvider;
            _seriesProvider = seriesProvider;
        }

        [HttpPost]
        public JsonResult ScanNewSeries()
        {
            _syncProvider.BeginUpdateNewSeries();
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
                defaultQuality); ;

            return PartialView("AddSeriesItem", suggestions);

        }

        public JsonResult AddSeries(string path, int seriesId, int qualityProfileId)
        {
            //Get TVDB Series Name
            //Create new folder for series
            //Add the new series to the Database

            _seriesProvider.AddSeries(path.Replace('|', Path.DirectorySeparatorChar).Replace('^', Path.VolumeSeparatorChar), seriesId, qualityProfileId);
            ScanNewSeries();
            return new JsonResult() { Data = "ok" };
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
            //var bestResult = _tvDbProvider.GetBestMatch(dataVal.ToList(), searchString);

            return new SelectList(dataVal, "Id", "SeriesName", dataVal[0].Id);
        }

    }
}
