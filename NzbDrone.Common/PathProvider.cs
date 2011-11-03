using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NzbDrone.Common
{
    public class PathProvider
    {

        private const string WEB_FOLDER = "NzbDrone.Web";
        private const string APP_DATA = "App_Data";

        private const string LOG_CONFIG_FILE = "log.config";
        private const string APP_CONFIG_FILE = "config.xml";

        private const string NZBDRONE_DB_FILE = "nzbdrone.sdf";
        private const string LOG_DB_FILE = "log.sdf";

        public const string UPDATE_SANDBOX_FOLDER_NAME = "nzbdrone_update";

        private readonly string _applicationPath;

        
        public PathProvider(EnviromentProvider enviromentProvider)
        {
            _applicationPath = enviromentProvider.ApplicationPath;
        }

        public PathProvider()
        {
            
        }

        public virtual String LogPath
        {
            get { return Environment.CurrentDirectory; }
        }

        public virtual string WebRoot
        {
            get
            {
                return Path.Combine(_applicationPath, WEB_FOLDER);
            }
        }

        public virtual string AppData
        {
            get
            {
                var path = Path.Combine(WebRoot, APP_DATA);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public virtual string NzbDronoeDbFile
        {
            get
            {

                return Path.Combine(AppData, NZBDRONE_DB_FILE);
            }
        }

        public virtual string LogDbFile
        {
            get
            {

                return Path.Combine(AppData, LOG_DB_FILE);
            }
        }

        public virtual String SystemTemp
        {
            get
            {
                return Path.GetTempPath();
            }
        }

        public string LogConfigFile
        {
            get { return Path.Combine(WebRoot, LOG_CONFIG_FILE); }
        }

        public string AppConfigFile
        {
            get { return Path.Combine(_applicationPath, APP_CONFIG_FILE); }
        }

        public string BannerPath
        {
            get { return Path.Combine(WebRoot, "Content", "Images", "Banners"); }
        }

        public string CacheFolder
        {
            get { return Path.Combine(AppData, "Cache"); }
        }

        public string UpdateSandboxFolder
        {
            get { return Path.Combine(SystemTemp, UPDATE_SANDBOX_FOLDER_NAME); }
        }
    }
}