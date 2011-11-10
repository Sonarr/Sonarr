using System.IO;
using AutoMoq;
using Moq;
using NUnit.Framework;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Test.Common;
using PetaPoco;

namespace NzbDrone.Core.Test.Framework
{
    public class TestBase : LoggingTest
    // ReSharper disable InconsistentNaming
    {
        static TestBase()
        {
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
        protected IDatabase Db = null;

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
            InitLogging();

            ExceptionVerification.Reset();
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }

            Directory.CreateDirectory(TempFolder);

            LiveKernel = new StandardKernel();
            Mocker = new AutoMoqer();
        }

        protected void WithStrictMocker()
        {
            Mocker = new AutoMoqer(MockBehavior.Strict);
            if (Db != null)
            {
                Mocker.SetConstant(Db);
            }
        }

        protected void WithRealDb()
        {
            Db = MockLib.GetEmptyDatabase();
            Mocker.SetConstant(Db);
        }

        [TearDown]
        public void TearDownBase()
        {
            ExceptionVerification.AssertNoUnexcpectedLogs();
            Mocker = new AutoMoqer(MockBehavior.Strict);
            WebTimer.Stop();
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
