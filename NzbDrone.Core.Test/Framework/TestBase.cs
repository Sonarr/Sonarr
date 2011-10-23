using System.IO;
using NUnit.Framework;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Core.Test.Framework
{
    public class TestBase
    // ReSharper disable InconsistentNaming
    {

        [SetUp]
        public virtual void Setup()
        {
            ExceptionVerification.Reset();
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            ExceptionVerification.AssertNoUnexcpectedLogs();
        }


        protected string TempFolder
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "_temp"); }
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
