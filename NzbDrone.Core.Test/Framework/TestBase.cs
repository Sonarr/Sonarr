using System.IO;
using AutoMoq;
using NUnit.Framework;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Framework
{
    public class TestBase : LoggingTest
    // ReSharper disable InconsistentNaming
    {

        static TestBase()
        {
            InitLogging();

            var oldDbFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sdf", SearchOption.AllDirectories);
            foreach (var file in oldDbFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            MockLib.CreateDataBaseTemplate();
        }

        protected StandardKernel LiveKernel = null;
        protected AutoMoqer Mocker = null;

        protected string VirtualPath
        {
            get
            {
                var virtualPath = Path.Combine(TempFolder, "VirtualNzbDrone");
                if (!Directory.Exists(virtualPath)) Directory.CreateDirectory(virtualPath);

                return virtualPath;
            }
        }

        [SetUp]
        public virtual void SetupBase()
        {
            ExceptionVerification.Reset();
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }

            Directory.CreateDirectory(TempFolder);

            LiveKernel = new StandardKernel();
            Mocker = new AutoMoqer();
        }

        [TearDown]
        public void TearDownBase()
        {
            ExceptionVerification.AssertNoUnexcpectedLogs();
        }


        protected void WithTempAsStartUpPath()
        {
            Mocker.GetMock<EnviromentProvider>()
                .SetupGet(c => c.ApplicationPath)
                .Returns(VirtualPath);

            Mocker.Resolve<PathProvider>();
        }


        protected string TempFolder
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "temp"); }
        }

        protected string GetTestFilePath(string fileName)
        {
            return Path.Combine(@".\Files\", fileName);
        }

        protected string ReadTestFile(string fileName)
        {
            return File.ReadAllText(GetTestFilePath(fileName));
        }
    }
}
