using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace NzbDrone.Web.UI.Automation
{
    [Explicit]
    [TestFixture(Category = "Automation")]
    public abstract class AutomationTestBase
    {
        private static readonly EnviromentProvider enviromentProvider = new EnviromentProvider();

        private readonly string _clonePackagePath;
        private readonly string _masterPackagePath;

        protected string AppUrl
        {
            get
            {
                return "http://localhost:8989";
            }
        }

        protected AutomationTestBase()
        {
            var rawPackagePath = Path.Combine(enviromentProvider.ApplicationPath, "_rawPackage");
            _clonePackagePath = Path.Combine(rawPackagePath, "NzbDrone_Automation");
            _masterPackagePath = Path.Combine(rawPackagePath, "NzbDrone");
        }


        protected RemoteWebDriver Driver { get; private set; }



        [SetUp]
        public void AutomationSetup()
        {

        }

        [TearDown]
        public void AutomationTearDown()
        {


            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Screenshots"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Screenshots");
            }

            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*__*.png").Select(c => new FileInfo(c)))
            {
                File.Copy(file.FullName, Directory.GetCurrentDirectory() + "\\Screenshots\\" + file.Name, true);
                file.Delete();
            }
        }


        [TestFixtureSetUp]
        public void AutomationTestFixtureSetup()
        {
            StopNzbDrone();


            DeleteClone();
            ClonePackage();

            //StartNzbDrone();
            InstallNzbDroneService();

            new HttpProvider().DownloadString(AppUrl);

            Driver = new FirefoxDriver();
        }



        [TestFixtureTearDown]
        public void AutomationTestFixtureTearDown()
        {
            Driver.Close();
            StopNzbDrone();
        }


        protected void CaptureScreen()
        {
            var method = new StackFrame(1).GetMethod().Name;

            var fileName = String.Format("{0}__{1}.png", GetType().Name, method);

            ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(fileName, ImageFormat.Png);
        }



        private void StartNzbDrone()
        {
            StartProcess("nzbdrone.exe", false);

        }

        private void StopNzbDrone()
        {

            foreach (var process in Process.GetProcesses())
            {
                if (string.Equals(process.ProcessName, "NzbDrone", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(process.ProcessName, "IISExpress", StringComparison.InvariantCultureIgnoreCase))
                    process.Kill();
                process.WaitForExit();
            }

            try
            {
                StartProcess("ServiceUninstall.exe", true);
            }
            catch { }

            foreach (var process in Process.GetProcesses())
            {
                if (string.Equals(process.ProcessName, "NzbDrone", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(process.ProcessName, "IISExpress", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(process.ProcessName, "ServiceUninstall", StringComparison.InvariantCultureIgnoreCase))
                    process.Kill();
                process.WaitForExit();
            }

        }

        private void InstallNzbDroneService()
        {
            StartProcess("ServiceInstall.exe", true);
        }

        private void StartProcess(string fileName, bool waitForExit)
        {

            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(_clonePackagePath, fileName),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var nzbDroneProcess = new Process
            {
                StartInfo = startInfo
            };

            nzbDroneProcess.OutputDataReceived += (o, args) => Console.WriteLine(args.Data);
            nzbDroneProcess.ErrorDataReceived += (o, args) => Console.WriteLine(args.Data);

            nzbDroneProcess.Start();

            nzbDroneProcess.BeginErrorReadLine();
            nzbDroneProcess.BeginOutputReadLine();

            if (waitForExit)
            {
                nzbDroneProcess.WaitForExit();
            }
        }


        private void ClonePackage()
        {
            new DiskProvider().CopyDirectory(_masterPackagePath, _clonePackagePath);
        }

        private void DeleteClone()
        {
            if (Directory.Exists(_clonePackagePath))
            {
                Directory.Delete(_clonePackagePath, true);
            }
        }

        private string CreatePackage()
        {
            Console.WriteLine("Creating NzbDrone Package");

            StopNzbDrone();

            var rootDirectory = new DirectoryInfo(enviromentProvider.ApplicationPath);

            if (rootDirectory.GetDirectories("_rawPackage").Any())
            {
                rootDirectory.GetDirectories("_rawPackage").ToList().ForEach(c => c.Delete(true));
            }

            var startInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine(rootDirectory.FullName, "package.bat"),
                                    WorkingDirectory = rootDirectory.FullName
                                };

            Process.Start(startInfo).WaitForExit();

            var testFolder = Path.Combine(enviromentProvider.SystemTemp, "NzbDroneAutomation");

            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }

            Directory.Move(Path.Combine(rootDirectory.FullName, "_rawPackage", "nzbdrone"), testFolder);



            return testFolder;
        }

        private void ResetUserData()
        {
            var appDataPath = Path.Combine(_clonePackagePath, "NzbDrone.Web", "app_data");

            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }


        private static void CleanBinFolder()
        {
            var folderName = "Debug";

            if (EnviromentProvider.IsDebug)
            {
                folderName = "Release";
            }

            var dirs = Directory.GetDirectories(enviromentProvider.ApplicationPath, folderName, SearchOption.AllDirectories);


            foreach (var dir in dirs)
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
