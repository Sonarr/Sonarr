using System;
using System.IO;
using System.Reflection;

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
        private readonly IDiskProvider _diskProvider;

        public AppFolderInfo(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
            AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.DoNotVerify), "NzbDrone");
            StartUpFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            TempFolder = Path.GetTempPath();

            if (!_diskProvider.FolderExists(AppDataFolder))
            {
                MigrateFromAppDate();
            }
        }


        private void MigrateFromAppDate()
        {
            var oldAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "NzbDrone");

            if (_diskProvider.FolderExists(oldAppDataFolder))
            {
                _diskProvider.MoveFolder(oldAppDataFolder, AppDataFolder);
            }
            else
            {
                _diskProvider.CreateFolder(AppDataFolder);
            }
        }

        public string AppDataFolder { get; private set; }

        public string StartUpFolder { get; private set; }

        public String TempFolder { get; private set; }
    }
}