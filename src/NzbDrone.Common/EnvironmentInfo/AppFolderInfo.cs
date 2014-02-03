using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IAppFolderInfo
    {
        string AppDataFolder { get; }
        string TempFolder { get; }
        string StartUpFolder { get; }
    }

    public class AppFolderInfo : IAppFolderInfo
    {
        private readonly Environment.SpecialFolder DATA_SPECIAL_FOLDER = Environment.SpecialFolder.CommonApplicationData;

        public AppFolderInfo(IStartupContext startupContext)
        {
            if (OsInfo.IsLinux)
            {
                DATA_SPECIAL_FOLDER = Environment.SpecialFolder.ApplicationData;
            }

            if (startupContext.Args.ContainsKey(StartupContext.APPDATA))
            {
                AppDataFolder = startupContext.Args[StartupContext.APPDATA];
            }
            else
            {
                AppDataFolder = Path.Combine(Environment.GetFolderPath(DATA_SPECIAL_FOLDER, Environment.SpecialFolderOption.None), "NzbDrone");
            }

            StartUpFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            TempFolder = Path.GetTempPath();
        }

        public string AppDataFolder { get; private set; }

        public string StartUpFolder { get; private set; }

        public String TempFolder { get; private set; }
    }
}