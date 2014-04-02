using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using Nancy;
using Nancy.Responses;

namespace NzbDrone.Api.Logs
{
    public class LogFileModule : NzbDroneRestModule<LogFileResource>
    {
        private const string LOGFILE_ROUTE = @"/(?<filename>nzbdrone(?:\.\d+)?\.txt)";

        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public LogFileModule(IAppFolderInfo appFolderInfo,
                             IDiskProvider diskProvider)
            : base("log/file")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            GetResourceAll = GetLogFiles;

            Get[LOGFILE_ROUTE] = options => GetLogFile(options.filename);
        }

        private List<LogFileResource> GetLogFiles()
        {
            var result = new List<LogFileResource>();
            
            var files = _diskProvider.GetFiles(_appFolderInfo.GetLogFolder(), SearchOption.TopDirectoryOnly);

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                
                result.Add(new LogFileResource
                {
                    Id = i + 1,
                    Filename = Path.GetFileName(file),
                    LastWriteTime = _diskProvider.FileGetLastWriteUtc(file)
                });
            }

            return result.OrderByDescending(l => l.LastWriteTime).ToList();
        }

        private Response GetLogFile(string filename)
        {
            var filePath = Path.Combine(_appFolderInfo.GetLogFolder(), filename);

            if (!_diskProvider.FileExists(filePath))
                return new NotFoundResponse();

            var data = _diskProvider.ReadAllText(filePath);
            
            return new TextResponse(data);
        }
    }
}