using System;
using System.Linq;
using System.IO;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Test.Common
{
    public class TestBase : LoggingTest
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
        public void TestBaseSetup()
        {
            MockedRestProvider = new Mock<RestProvider>();
            ReportingService.RestProvider = MockedRestProvider.Object;
            ReportingService.SetupExceptronDriver();


            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }

            Directory.CreateDirectory(TempFolder);
        }

        [TearDown]
        public void TestBaseTearDown()
        {
            _mocker = null;
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
