using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Linq;
using NLog;
using NzbDrone.Core.Helpers;
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
        private readonly SyncProvider _syncProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly DiskProvider _diskProvider;

        public AddSeriesController(SyncProvider syncProvider, RootDirProvider rootFolderProvider,
                                   ConfigProvider configProvider,
                                   QualityProvider qualityProvider, TvDbProvider tvDbProvider,
                                   SeriesProvider seriesProvider, JobProvider jobProvider,
                                   DiskProvider diskProvider)
        {
            _syncProvider = syncProvider;
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
            var rootDirs =_rootFolderProvider.GetAll().Select(r =>
                        new RootDirModel
                        {
                            Path = r.Path,
                            CleanPath = r.Path.Replace(Path.DirectorySeparatorChar, '|').Replace(Path.VolumeSeparatorChar, '^').Replace('\'', '`')
                        }).ToList();
            ViewData["RootDirs"] = rootDirs;
            ViewData["DirSep"] = Path.DirectorySeparatorChar.ToString().Replace(Path.DirectorySeparatorChar, '|');

            var defaultQuality = _configProvider.DefaultQualityProfile;
            var qualityProfiles = _qualityProvider.GetAllProfiles();

            ViewData["quality"] = new SelectList(
                qualityProfiles,
                "QualityProfileId",
                "Name",
                defaultQuality);

            return View();
        }

        public ActionResult Index()
        {
            var rootDirs = _rootFolderProvider.GetAll();

            var profiles = _qualityProvider.GetAllProfiles();
            var defaultQuality = Convert.ToInt32(_configProvider.DefaultQualityProfile);
            var selectList = new SelectList(profiles, "QualityProfileId", "Name", defaultQuality);
            ViewData["qualities"] = selectList;

            return View(rootDirs);
        }

        public ActionResult AddExisting()
        {
            var rootDirs = _rootFolderProvider.GetAll();

            var unmappedList = new List<String>();

            foreach (var folder in rootDirs)
            {
                unmappedList.AddRange(_rootFolderProvider.GetUnmappedFolders(folder.Path));
            }

            return View(unmappedList);
        }

        public ActionResult RenderPartial(string path)
        {
            var suggestions = GetSuggestionList(new DirectoryInfo(path).Name);

            ViewData["guid"] = Guid.NewGuid();
            ViewData["path"] = path;
            ViewData["javaPath"] = path.Replace(Path.DirectorySeparatorChar, '|').Replace(Path.VolumeSeparatorChar, '^').Replace('\'', '`');

            var defaultQuality = _configProvider.DefaultQualityProfile;
            var qualityProfiles = _qualityProvider.GetAllProfiles();

            ViewData["quality"] = new SelectList(
                qualityProfiles,
                "QualityProfileId",
                "Name",
                defaultQuality);

            return PartialView("AddSeriesItem", suggestions);
        }

        [HttpPost]
        public JsonResult AddNewSeries(string rootPath, string seriesName, int seriesId, int qualityProfileId)
        {
            try
            {
                var path =
                    rootPath.Replace('|', Path.DirectorySeparatorChar).Replace('^', Path.VolumeSeparatorChar).Replace(
                        '`', '\'') +
                    Path.DirectorySeparatorChar + EpisodeRenameHelper.CleanFilename(seriesName);

                //Create the folder for the new series and then Add it
                _diskProvider.CreateDirectory(path);

                _seriesProvider.AddSeries(path, seriesId, qualityProfileId);
                ScanNewSeries();
                return new JsonResult {Data = "ok"};
            }

            catch(Exception ex)
            {
                return new JsonResult { Data = "failed" };
            }
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

        [HttpPost]
        public JsonResult SaveRootDir(int id, string path)
        {
            try
            {
                _rootFolderProvider.Update(new RootDir { Id = id, Path = path });
            }
            catch (Exception ex)
            {
                Logger.Debug("Failed to save Root Dir");
                Logger.DebugException(ex.Message, ex);

                return new JsonResult { Data = "failed" };
            }

            return new JsonResult { Data = "ok" };
        }

        public ViewResult AddRootDir()
        {
            var rootDir = new RootDir { Path = String.Empty };

            var id = _rootFolderProvider.Add(rootDir);
            rootDir.Id = id;

            ViewData["RootDirId"] = id;

            return View("RootDir", rootDir);
        }

        public ActionResult GetRootDirView(RootDir rootDir)
        {
            ViewData["RootDirId"] = rootDir.Id;

            return PartialView("RootDir", rootDir);
        }

        public JsonResult DeleteRootDir(int rootDirId)
        {
            try
            {
                _rootFolderProvider.Remove(rootDirId);
            }

            catch (Exception)
            {
                return new JsonResult { Data = "failed" };
            }

            return new JsonResult { Data = "ok" };
        }

        public JsonResult JsonAutoCompletePath(string term)
        {
            var windowsSep = term.LastIndexOf('\\');

            if (windowsSep > -1)
            {
                var start = term.Substring(windowsSep + 1);
                var dirs = _diskProvider.GetDirectories(term.Substring(0, windowsSep + 1)).Where(d => new DirectoryInfo(d).Name.ToLower().StartsWith(start.ToLower())).Take(10);
                return Json(dirs.ToArray(), JsonRequestBehavior.AllowGet);
            }

            var index = term.LastIndexOf('/');

            if (index > -1)
            {
                var start = term.Substring(index + 1);
                var dirs = _diskProvider.GetDirectories(term.Substring(0, index + 1)).Where(d => new DirectoryInfo(d).Name.ToLower().StartsWith(start.ToLower())).Take(10);
                return Json(dirs.ToArray(), JsonRequestBehavior.AllowGet);
            }

            return Json(new JsonResult(), JsonRequestBehavior.AllowGet);
        }
    }
}