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
        public string WorkingDirectory
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "NzbDrone"); }
        }

        public string StartUpPath
        {
            get
            {
                var path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                return path;
            }
        }

        public String SystemTemp
        {
            get
            {
                return Path.GetTempPath();
            }
        }
    }
}