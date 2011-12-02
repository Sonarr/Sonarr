using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Web.Models;
using Telerik.Web.Mvc;

namespace NzbDrone.Web.Controllers
{
    public class SystemController : Controller
    {
        private readonly JobProvider _jobProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;

        public SystemController(JobProvider jobProvider, IndexerProvider indexerProvider, ConfigProvider configProvider, DiskProvider diskProvider)
        {
            _jobProvider = jobProvider;
            _indexerProvider = indexerProvider;
            _configProvider = configProvider;
            _diskProvider = diskProvider;
        }

        public ActionResult Jobs()
        {
            ViewData["Queue"] = _jobProvider.Queue.Select(c => new JobQueueItemModel {
                                                                                        Name = c.JobType.Name,
                                                                                        TargetId = c.TargetId,
                                                                                        SecondaryTargetId = c.SecondaryTargetId
                                                                                    });
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

        //PostDownloadView
        public ActionResult PendingProcessing()
        {
            ViewData["DropDir"] = _configProvider.SabDropDirectory;
            return View();
        }

        [GridAction]
        public ActionResult _PendingProcessingAjaxBinding()
        {
            var dropDir = _configProvider.SabDropDirectory;
            var subFolders = _diskProvider.GetDirectories(dropDir);

            var models = new List<PendingProcessingModel>();

            //Get the CreationTime and Files
            foreach (var folder in subFolders)
            {
                var model = new PendingProcessingModel();
                model.Name = new DirectoryInfo(folder).Name;
                model.Created = _diskProvider.DirectoryDateCreated(folder);
                model.Path = folder.Replace(Path.DirectorySeparatorChar, '|').Replace(Path.VolumeSeparatorChar, '^').Replace('\'', '`');

                var files = _diskProvider.GetFileInfos(folder, "*.*", SearchOption.AllDirectories);

                var fileResult = "<div><div style=\"width: 600px; display: inline-block;\"><b>Name</b></div><div style=\"display: inline-block;\"><b>Size</b></div></div>";

                foreach (var fileInfo in files)
                {
                    fileResult += String.Format("<div><div style=\"width: 600px; display: inline-block;\">{0}</div><div style=\"display: inline-block;\">{1}</div></div>", fileInfo.Name,
                                                FileSizeFormatHelper.Format(fileInfo.Length, 1));
                }

                model.Files = fileResult;

                models.Add(model);
            }

            return View(new GridModel(models));
        }

        public JsonResult RenamePendingProcessing(string path)
        {
            path = path.Replace('|', Path.DirectorySeparatorChar).Replace('^', Path.VolumeSeparatorChar).Replace('`', '\'');

            var di = new DirectoryInfo(path);
            var dropDir = di.Parent.FullName;
            var folder = di.Name;

            if (!folder.StartsWith("_UNPACK_") && !folder.StartsWith("_FAILED_"))
                return new JsonResult { Data = "no change" };

            folder = folder.Substring(8);
            var newPath = dropDir + Path.DirectorySeparatorChar + folder;
            _diskProvider.MoveDirectory(path, newPath);

            return new JsonResult { Data = "ok" };
        }
    }
}
