using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Logs
{
    public class LogFileModule : NzbDroneRestModule<LogFileResource>
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public LogFileModule(IAppFolderInfo appFolderInfo,
                             IDiskProvider diskProvider)
            : base("log/files")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            GetResourceAll = GetLogFiles;
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
                    LastWriteTime = _diskProvider.GetLastFileWrite(file)
                });
            }

            return result.OrderByDescending(l => l.LastWriteTime).ToList();
        }
    }
}