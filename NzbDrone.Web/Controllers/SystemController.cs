using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using NzbDrone.Common;
using NzbDrone.Core;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.DownloadClients;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class SystemController : Controller
    {
        private readonly JobProvider _jobProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;
        private readonly BackupProvider _backupProvider;

        public SystemController(JobProvider jobProvider, IndexerProvider indexerProvider,
                                    ConfigProvider configProvider, DiskProvider diskProvider,
                                    BackupProvider backupProvider)
        {
            _jobProvider = jobProvider;
            _indexerProvider = indexerProvider;
            _configProvider = configProvider;
            _diskProvider = diskProvider;
            _backupProvider = backupProvider;
        }

        public ActionResult Jobs()
        {
            var queue = _jobProvider.Queue.Select(c => new JobQueueItemModel
            {
                Name = c.JobType.Name,
                TargetId = c.TargetId,
                SecondaryTargetId = c.SecondaryTargetId
            });

            var serializedQueue = new JavaScriptSerializer().Serialize(queue);

            ViewData["Queue"] = serializedQueue;

            var jobs = _jobProvider.All().Select(j => new JobModel
                                                          {
                                                              Id = j.Id,
                                                              Enable = j.Enable,
                                                              TypeName = j.TypeName,
                                                              Name = j.Name,
                                                              Interval = j.Interval,
                                                              LastExecution = j.LastExecution.ToString(),
                                                              Success = j.Success
                                                          }).OrderBy(j => j.Interval);

            var serializedJobs = new JavaScriptSerializer().Serialize(jobs);

            return View((object)serializedJobs);
        }

        public ActionResult Indexers()
        {
            var indexers = _indexerProvider.All();
            var serialized = new JavaScriptSerializer().Serialize(indexers);

            return View((object)serialized);
        }

        public ActionResult Config()
        {
            var config = _configProvider.All();
            var serialized = new JavaScriptSerializer().Serialize(config);

            return View((object)serialized);
        }

        public JsonResult SelectConfigAjax()
        {
            var config = _configProvider.All();

            return Json(new
                            {
                                iTotalRecords = config.Count(),
                                iTotalDisplayRecords = config.Count(),
                                aaData = config
                            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string SaveConfigAjax(string id, string value)
        {
            _configProvider.SetValue(id, value);
            return value;
        }

        [HttpPost]
        public string InsertConfigAjax(string key, string value)
        {
            _configProvider.SetValue(key, value);
            return key;
        }

        //PostDownloadView
        public ActionResult PendingProcessing()
        {
            ViewData["DropDir"] = _configProvider.SabDropDirectory;

            var dropDir = _configProvider.SabDropDirectory;
            var subFolders = _diskProvider.GetDirectories(dropDir);

            var models = new List<PendingProcessingModel>();

            //Get the CreationTime and Files
            foreach (var folder in subFolders)
            {
                var model = new PendingProcessingModel();
                model.Name = new DirectoryInfo(folder).Name;
                model.Created = _diskProvider.DirectoryDateCreated(folder).ToString();
                model.Path = folder.Replace(Path.DirectorySeparatorChar, '|').Replace(Path.VolumeSeparatorChar, '^').Replace('\'', '`');

                var files = _diskProvider.GetFileInfos(folder, "*.*", SearchOption.AllDirectories);

                var fileResult = "<div><div style=\"width: 600px; display: inline-block;\"><b>Name</b></div><div style=\"display: inline-block;\"><b>Size</b></div></div>";

                foreach (var fileInfo in files)
                {
                    fileResult += String.Format("<div><div style=\"width: 600px; display: inline-block;\">{0}</div><div style=\"display: inline-block;\">{1}</div></div>", fileInfo.Name,
                                                fileInfo.Length.ToBestFileSize(1));
                }

                model.Files = fileResult;

                models.Add(model);
            }

            var serialized = new JavaScriptSerializer().Serialize(models);

            return View((object)serialized);
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

        public JsonResult RunJob(string typeName)
        {
            if (!_jobProvider.QueueJob(typeName))
                return JsonNotificationResult.Oops("Invalid Job Name");

            return JsonNotificationResult.Info("Job Queued");
        }

        public ActionResult Backup()
        {
            var file = _backupProvider.CreateBackupZip();
            var fileInfo = new FileInfo(file);

            return File(fileInfo.FullName, "application/binary", fileInfo.Name);
        }
    }
}
