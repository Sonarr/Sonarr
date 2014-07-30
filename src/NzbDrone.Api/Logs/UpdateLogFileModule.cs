using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Logs
{
    public class UpdateLogFileModule : LogFileModuleBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public UpdateLogFileModule(IAppFolderInfo appFolderInfo,
                                   IDiskProvider diskProvider,
                                   IConfigFileProvider configFileProvider)
            : base(diskProvider, configFileProvider, "/update")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        protected override IEnumerable<String> GetLogFiles()
        {
            if (!_diskProvider.FolderExists(_appFolderInfo.GetUpdateLogFolder())) return Enumerable.Empty<String>();

            return _diskProvider.GetFiles(_appFolderInfo.GetUpdateLogFolder(), SearchOption.TopDirectoryOnly)
                                     .Where(f => Regex.IsMatch(Path.GetFileName(f), LOGFILE_ROUTE.TrimStart('/'), RegexOptions.IgnoreCase))
                                     .ToList();
        }

        protected override String GetLogFilePath(String filename)
        {
            return Path.Combine(_appFolderInfo.GetUpdateLogFolder(), filename);
        }

        protected override String DownloadUrlRoot
        {
            get
            {
                return "updatelogfile";
            }
        }
    }
}