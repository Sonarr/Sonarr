using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NzbDrone.Common
{
    public static class PathExtentions
    {
        private const string WEB_FOLDER = "NzbDrone.Web\\";
        private const string APP_DATA = "App_Data\\";
  

        private const string LOG_CONFIG_FILE = "log.config";
        private const string APP_CONFIG_FILE = "config.xml";

        private const string NZBDRONE_DB_FILE = "nzbdrone.sdf";
        private const string LOG_DB_FILE = "log.sdf";

        private const string UPDATE_SANDBOX_FOLDER_NAME = "nzbdrone_update\\";
        private const string UPDATE_PACKAGE_FOLDER_NAME = "nzbdrone\\";
        private const string UPDATE_BACKUP_FOLDER_NAME = "nzbdrone_backup\\";

        public static string GetUpdateSandboxFolder(this PathProvider pathProvider)
        {
            return Path.Combine(pathProvider.SystemTemp, UPDATE_SANDBOX_FOLDER_NAME);
        }

        public static string GetUpdateBackUpFolder(this PathProvider pathProvider)
        {
            return Path.Combine(pathProvider.GetUpdateSandboxFolder(), UPDATE_BACKUP_FOLDER_NAME);
        }

        public static string GetUpdatePackageFolder(this PathProvider pathProvider)
        {
            return Path.Combine(pathProvider.GetUpdateSandboxFolder(), UPDATE_PACKAGE_FOLDER_NAME);
        }
    }
}