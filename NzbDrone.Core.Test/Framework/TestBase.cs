using System.IO;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Framework
{
    public class TestBase
    // ReSharper disable InconsistentNaming
    {

        [SetUp]
        public virtual void SetupBase()
        {
            ExceptionVerification.Reset();
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }

            Directory.CreateDirectory(TempFolder);
        }

        [TearDown]
        public void TearDownBase()
        {
            ExceptionVerification.AssertNoUnexcpectedLogs();
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
