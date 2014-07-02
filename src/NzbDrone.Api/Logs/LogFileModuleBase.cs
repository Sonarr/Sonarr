using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Disk;
using Nancy;
using Nancy.Responses;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Logs
{
    public abstract class LogFileModuleBase : NzbDroneRestModule<LogFileResource>
    {
        protected const string LOGFILE_ROUTE = @"/(?<filename>[-.a-zA-Z0-9]+?\.txt)";

        private readonly IDiskProvider _diskProvider;
        private readonly IConfigFileProvider _configFileProvider;

        public LogFileModuleBase(IDiskProvider diskProvider,
                                 IConfigFileProvider configFileProvider,
                                 String route)
            : base("log/file" + route)
        {
            _diskProvider = diskProvider;
            _configFileProvider = configFileProvider;
            GetResourceAll = GetLogFilesResponse;

            Get[LOGFILE_ROUTE] = options => GetLogFileResponse(options.filename);
        }

        private List<LogFileResource> GetLogFilesResponse()
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
                    LastWriteTime = _diskProvider.FileGetLastWriteUtc(file),
                    ContentsUrl = String.Format("{0}/api/{1}/{2}", _configFileProvider.UrlBase, Resource, filename),
                    DownloadUrl = String.Format("{0}/{1}/{2}", _configFileProvider.UrlBase, DownloadUrlRoot, filename)
                });
            }

            return result.OrderByDescending(l => l.LastWriteTime).ToList();
        }

        private Response GetLogFileResponse(String filename)
        {
            var filePath = GetLogFilePath(filename);

            if (!_diskProvider.FileExists(filePath))
                return new NotFoundResponse();

            var data = _diskProvider.ReadAllText(filePath);
            
            return new TextResponse(data);
        }

        protected abstract IEnumerable<String> GetLogFiles();
        protected abstract String GetLogFilePath(String filename);

        protected abstract String DownloadUrlRoot { get; }
    }
}