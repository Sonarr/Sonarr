using System;
using System.IO;
using System.Reflection;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IAppDirectoryInfo
    {
        string WorkingDirectory { get; }
        string SystemTemp { get; }
        string StartUpPath { get; }
    }

    public class AppDirectoryInfo : IAppDirectoryInfo
    {

        public AppDirectoryInfo()
        {
            WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "NzbDrone");
            StartUpPath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            SystemTemp = Path.GetTempPath();

            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }
        }

        public string WorkingDirectory { get; private set; }

        public string StartUpPath { get; private set; }

        public String SystemTemp { get; private set; }
    }
}