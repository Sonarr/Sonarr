using System;
using System.Linq;
using System.IO;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Test.Common
{
    public abstract class TestBase : LoggingTest
    {
        protected const string INTEGRATION_TEST = "Integration Test";

        private AutoMoqer _mocker;
        protected AutoMoqer Mocker
        {
            get
            {
                if (_mocker == null)
                {
                    _mocker = new AutoMoqer();
                }

                return _mocker;
            }
        }

        protected Mock<RestProvider> MockedRestProvider { get; private set; }

        private string VirtualPath
        {
            get
            {
                var virtualPath = Path.Combine(TempFolder, "VirtualNzbDrone");
                if (!Directory.Exists(virtualPath)) Directory.CreateDirectory(virtualPath);

                return virtualPath;
            }
        }

        protected string TempFolder { get; private set; }

        [SetUp]
        public void TestBaseSetup()
        {

            Mocker.SetConstant(LogManager.GetLogger("TestLogger"));

            TempFolder = Path.Combine(Directory.GetCurrentDirectory(), "_temp_" + DateTime.Now.Ticks);

            MockedRestProvider = new Mock<RestProvider>();
            ReportingService.RestProvider = MockedRestProvider.Object;
            ReportingService.SetupExceptronDriver();

            Directory.CreateDirectory(TempFolder);
        }

        [TearDown]
        public void TestBaseTearDown()
        {
            _mocker = null;

            try
            {
                if (Directory.Exists(TempFolder))
                {
                    Directory.Delete(TempFolder, true);
                }
            }
            catch (Exception)
            {
            }

        }

        protected void WithStrictMocker()
        {
            if (_mocker != null)
                throw new InvalidOperationException("Can not switch to a strict container after container has been used. make sure this is the first call in your test.");

            _mocker = new AutoMoqer(MockBehavior.Strict);
        }

        protected void WithTempAsAppPath()
        {
            Mocker.GetMock<EnvironmentProvider>()
                .SetupGet(c => c.ApplicationPath)
                .Returns(VirtualPath);
        }

        protected string GetTestFilePath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
        }
    }
}
