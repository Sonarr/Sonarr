using System;
using System.IO;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Integration.Test
{
    public class IntegrationTestFolderInfo : IAppFolderInfo
    {
        public IntegrationTestFolderInfo()
        {
            TempFolder = Path.GetTempPath();
            AppDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "integ_test", DateTime.Now.Ticks.ToString());

            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }

            StartUpFolder = Directory.GetCurrentDirectory();
        }

        public string AppDataFolder { get; private set; }
        public string TempFolder { get; private set; }
        public string StartUpFolder { get; private set; }
    }
}