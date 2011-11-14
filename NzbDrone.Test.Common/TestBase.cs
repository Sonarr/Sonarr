using System.Linq;
using System.IO;

using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Test.Common
{
    public class TestBase : LoggingTest
    // ReSharper disable InconsistentNaming
    {

        protected const string IntegrationTest = "Integration Test";

        protected AutoMoqer Mocker;

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
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }

            Directory.CreateDirectory(TempFolder);

            Mocker = new AutoMoqer();
        }

        [TearDown]
        public void TestBaseTearDown()
        {
            Mocker.VerifyAllMocks();
        }

        protected virtual void WithStrictMocker()
        {
            Mocker = new AutoMoqer(MockBehavior.Strict);
        }


        protected void WithTempAsAppPath()
        {
            Mocker.GetMock<EnviromentProvider>()
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
