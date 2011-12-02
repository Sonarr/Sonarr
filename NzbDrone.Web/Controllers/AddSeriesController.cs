using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class AddSeriesController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
        public JsonResult ScanNewSeries()
        {
            _jobProvider.QueueJob(typeof(ImportNewSeriesJob));
            return new JsonResult();
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
            path = Path.Combine(path, MediaFileProvider.CleanFilename(seriesName));

            //Create the folder for the new series
            //Use the created folder name when adding the series
            path = _diskProvider.CreateDirectory(path);

            return AddExistingSeries(path, seriesName, seriesId, qualityProfileId);
        }

        [HttpPost]
        public JsonResult AddExistingSeries(string path, string seriesName, int seriesId, int qualityProfileId)
        {
            try
            {
                _seriesProvider.AddSeries(path, seriesId, qualityProfileId);
                ScanNewSeries();
                return Json(new NotificationResult() { Title = seriesName, Text = "Was added successfully" });
            }

            catch (Exception ex)
            {
                return Json(new NotificationResult() { Title = "Failed", Text  = ex.Message, NotificationType = NotificationType.Error});
            }
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

        public JsonResult AddSeries(string path, int seriesId, int qualityProfileId)
        {
            //Get TVDB Series Name
            //Create new folder for series
            //Add the new series to the Database

            _seriesProvider.AddSeries(
                path.Replace('|', Path.DirectorySeparatorChar).Replace('^', Path.VolumeSeparatorChar).Replace('`', '\''), seriesId,
                qualityProfileId);
            ScanNewSeries();
            return new JsonResult { Data = "ok" };
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


        //Root Directory
        [HttpPost]
        public JsonResult SaveRootDir(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return new JsonResult { Data = "failed" };

            //Don't let a user add a rootDir that is the same as their SABnzbd TV Directory
            if (path.Equals(_configProvider.SabDropDirectory, StringComparison.InvariantCultureIgnoreCase))
                return new JsonResult { Data = "failed" };

            try
            {
                _rootFolderProvider.Add(new RootDir { Path = path });

            }
            catch (Exception ex)
            {
                Logger.Debug("Failed to save Root Dir");
                Logger.DebugException(ex.Message, ex);

                return new JsonResult { Data = "failed" };
            }

            return new JsonResult { Data = "ok" };
        }

        [HttpGet]
        public JsonResult LookupSeries(string term)
        {
            var tvDbResults = _tvDbProvider.SearchSeries(term).Select(r => new TvDbSearchResultModel
                    {
                        Id = r.Id,
                        Title = r.SeriesName,
                        FirstAired = r.FirstAired.ToShortDateString()
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

        public JsonResult DeleteRootDir(string path)
        {
            try
            {
                var id = _rootFolderProvider.GetAll().Where(c => c.Path == path).First().Id;
                _rootFolderProvider.Remove(id);
            }

            catch (Exception)
            {
                return new JsonResult { Data = "failed" };
            }

            return new JsonResult { Data = "ok" };
        }
    }
}