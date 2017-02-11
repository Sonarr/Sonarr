using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using Sonarr.Http;

namespace Sonarr.Api.V3.Logs
{
    public abstract class LogFileModuleBase : SonarrRestModule<LogFileResource>
    {
        protected const string LOGFILE_ROUTE = @"/(?<filename>[-.a-zA-Z0-9]+?\.txt)";

        private readonly IDiskProvider _diskProvider;
        private readonly IConfigFileProvider _configFileProvider;

        public LogFileModuleBase(IDiskProvider diskProvider,
                                 IConfigFileProvider configFileProvider,
                                 string route)
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
                    LastWriteTime = _diskProvider.FileGetLastWrite(file),
                    ContentsUrl = string.Format("{0}/api/v3/{1}/{2}", _configFileProvider.UrlBase, Resource, filename),
                    DownloadUrl = string.Format("{0}/{1}/{2}", _configFileProvider.UrlBase, DownloadUrlRoot, filename)
                });
            }

            return result.OrderByDescending(l => l.LastWriteTime).ToList();
        }

        private Response GetLogFileResponse(string filename)
        {
            var filePath = GetLogFilePath(filename);

            if (!_diskProvider.FileExists(filePath))
                return new NotFoundResponse();

            var data = _diskProvider.ReadAllText(filePath);
            
            return new TextResponse(data);
        }

        protected abstract IEnumerable<string> GetLogFiles();
        protected abstract string GetLogFilePath(string filename);

        protected abstract string DownloadUrlRoot { get; }
    }
}