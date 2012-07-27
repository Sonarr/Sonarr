using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Web.Filters;
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
        private readonly TvDbProvider _tvDbProvider;
        private readonly DiskProvider _diskProvider;

        public AddSeriesController(RootDirProvider rootFolderProvider,
                                   ConfigProvider configProvider,
                                   QualityProvider qualityProvider, TvDbProvider tvDbProvider,
                                   SeriesProvider seriesProvider, JobProvider jobProvider,
                                   DiskProvider diskProvider)
        {

            _rootFolderProvider = rootFolderProvider;
            _configProvider = configProvider;
            _qualityProvider = qualityProvider;
            _tvDbProvider = tvDbProvider;
            _seriesProvider = seriesProvider;
            _jobProvider = jobProvider;
            _diskProvider = diskProvider;
        }

        [HttpPost]
        public EmptyResult ScanNewSeries()
        {
            _jobProvider.QueueJob(typeof(ImportNewSeriesJob));
            return new EmptyResult();
        }

        public ActionResult AddNew()
        {
            ViewData["RootDirs"] = _rootFolderProvider.GetAll().Select(c => c.Path).OrderBy(e => e).ToList();

            var defaultQuality = _configProvider.DefaultQualityProfile;
            var qualityProfiles = _qualityProvider.All();

            ViewData["qualityProfiles"] = new SelectList(
                qualityProfiles,
                "QualityProfileId",
                "Name",
                defaultQuality);

            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExistingSeries()
        {
            var result = new ExistingSeriesModel();

            var unmappedList = new List<String>();

            foreach (var folder in _rootFolderProvider.GetAll())
            {
                unmappedList.AddRange(_rootFolderProvider.GetUnmappedFolders(folder.Path));
            }

            result.ExistingSeries = new List<Tuple<string, string, int>>();

            foreach (var folder in unmappedList)
            {
                var foldername = new DirectoryInfo(folder).Name;
                var tvdbResult = _tvDbProvider.SearchSeries(foldername).FirstOrDefault();

                var title = String.Empty;
                var seriesId = 0;
                if (tvdbResult != null)
                {
                    title = tvdbResult.SeriesName;
                    seriesId = tvdbResult.Id;
                }

                result.ExistingSeries.Add(new Tuple<string, string, int>(folder, title, seriesId));
            }

            var defaultQuality = Convert.ToInt32(_configProvider.DefaultQualityProfile);
            result.Quality = new SelectList(_qualityProvider.All(), "QualityProfileId", "Name", defaultQuality);

            return View(result);
        }

        [HttpPost]
        public JsonResult AddNewSeries(string path, string seriesName, int seriesId, int qualityProfileId)
        {
            if (string.IsNullOrWhiteSpace(path) || String.Equals(path,"null",StringComparison.InvariantCultureIgnoreCase)) 
                return JsonNotificationResult.Error("Couldn't add " + seriesName, "You need a valid root folder"); 

            path = Path.Combine(path, MediaFileProvider.CleanFilename(seriesName));

            //Create the folder for the new series
            //Use the created folder name when adding the series
            path = _diskProvider.CreateDirectory(path);

            return AddExistingSeries(path, seriesName, seriesId, qualityProfileId);
        }

        [HttpPost]
        [JsonErrorFilter]
        public JsonResult AddExistingSeries(string path, string seriesName, int seriesId, int qualityProfileId)
        {
            if (seriesId == 0 || String.IsNullOrWhiteSpace(seriesName))
                return JsonNotificationResult.Error("Add Existing series failed.", "Invalid Series information");

            _seriesProvider.AddSeries(seriesName,path, seriesId, qualityProfileId);
            ScanNewSeries();

            return JsonNotificationResult.Info(seriesName, "Was added successfully");
        }

        [HttpPost]
        public JsonResult QuickAddNewSeries(string seriesName, int seriesId, int qualityProfileId)
        {
            var path = _rootFolderProvider.GetMostFreeRootDir();
            path = Path.Combine(path, MediaFileProvider.CleanFilename(seriesName));

            //Create the folder for the new series
            //Use the created folder name when adding the series
            path = _diskProvider.CreateDirectory(path);

            return AddExistingSeries(path, seriesName, seriesId, qualityProfileId);
        }

        [ChildActionOnly]
        public ActionResult QuickAdd()
        {
            var defaultQuality = _configProvider.DefaultQualityProfile;
            var qualityProfiles = _qualityProvider.All();

            ViewData["qualityProfiles"] = new SelectList(
                qualityProfiles,
                "QualityProfileId",
                "Name",
                defaultQuality);

            return PartialView();
        }


        [HttpPost]
        [JsonErrorFilter]
        public JsonResult SaveRootDir(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                JsonNotificationResult.Error("Can't add root folder", "Path can not be empty");

            _rootFolderProvider.Add(new RootDir { Path = path });

            return JsonNotificationResult.Info("Root Folder saved", "Root folder saved successfully.");
        }

        [HttpGet]
        public JsonResult LookupSeries(string term)
        {
            var tvDbResults = _tvDbProvider.SearchSeries(term).Select(r => new TvDbSearchResultModel
                    {
                        Id = r.Id,
                        Title = r.SeriesName,
                        DisplayedTitle = r.FirstAired.Year > 1900 && !r.SeriesName.EndsWith("(" + r.FirstAired.Year + ")")
                                                        ?string.Format("{0} ({1})", r.SeriesName, r.FirstAired.Year)
                                                        :r.SeriesName,
                        Banner = r.Banner.BannerPath,
                        Url = String.Format("http://www.thetvdb.com/?tab=series&id={0}", r.Id)
                    }).ToList();

            return Json(tvDbResults, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RootList()
        {
            IEnumerable<String> rootDir = _rootFolderProvider.GetAll().Select(c => c.Path).OrderBy(e => e);
            return PartialView("RootList", rootDir);
        }

        public ActionResult RootDir()
        {
            return PartialView("RootDir");
        }

        [JsonErrorFilter]
        public JsonResult DeleteRootDir(string path)
        {

            var id = _rootFolderProvider.GetAll().Where(c => c.Path == path).First().Id;
            _rootFolderProvider.Remove(id);

            return null;
        }
    }
}