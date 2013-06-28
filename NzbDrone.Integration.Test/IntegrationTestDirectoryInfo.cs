using System;
using System.IO;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Integration.Test
{
    public class IntegrationTestDirectoryInfo : IAppDirectoryInfo
    {
        public IntegrationTestDirectoryInfo()
        {
            SystemTemp = Path.GetTempPath();
            WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "integ_test", DateTime.Now.Ticks.ToString());

            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }

            StartUpPath = Directory.GetCurrentDirectory();
        }

        public string WorkingDirectory { get; private set; }
        public string SystemTemp { get; private set; }
        public string StartUpPath { get; private set; }
    }
}