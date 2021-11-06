using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;

namespace Sonarr.Api.V3.Logs
{
    public abstract class LogFileControllerBase : Controller
    {
        protected const string LOGFILE_ROUTE = @"/(?<filename>[-.a-zA-Z0-9]+?\.txt)";
        protected string _resource;

        private readonly IDiskProvider _diskProvider;
        private readonly IConfigFileProvider _configFileProvider;

        public LogFileControllerBase(IDiskProvider diskProvider,
                                 IConfigFileProvider configFileProvider,
                                 string resource)
        {
            _diskProvider = diskProvider;
            _configFileProvider = configFileProvider;
            _resource = resource;
        }

        [HttpGet]
        public List<LogFileResource> GetLogFilesResponse()
        {
            var result = new List<LogFileResource>();

            var files = GetLogFiles().ToList();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var filename = Path.GetFileName(file);

                result.Add(new LogFileResource
                {
                    Id = i + 1,
                    Filename = filename,
                    LastWriteTime = _diskProvider.FileGetLastWrite(file),
                    ContentsUrl = string.Format("{0}/api/v1/{1}/{2}", _configFileProvider.UrlBase, _resource, filename),
                    DownloadUrl = string.Format("{0}/{1}/{2}", _configFileProvider.UrlBase, DownloadUrlRoot, filename)
                });
            }

            return result.OrderByDescending(l => l.LastWriteTime).ToList();
        }

        [HttpGet(@"{filename:regex([[-.a-zA-Z0-9]]+?\.txt)}")]
        public IActionResult GetLogFileResponse(string filename)
        {
            LogManager.Flush();

            var filePath = GetLogFilePath(filename);

            if (!_diskProvider.FileExists(filePath))
            {
                return NotFound();
            }

            return PhysicalFile(filePath, "text/plain");
        }

        protected abstract IEnumerable<string> GetLogFiles();
        protected abstract string GetLogFilePath(string filename);

        protected abstract string DownloadUrlRoot { get; }
    }
}
