using NUnit.Framework;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Core.Test.Framework
{
    public class TestBase
    // ReSharper disable InconsistentNaming
    {

        [SetUp]
        public void Setup()
        {
            ExceptionVerification.Reset();


        }

        [TearDown]
        public void TearDown()
        {
            JobProvider.Queue.Clear();
            ExceptionVerification.AssertNoUnexcpectedLogs();
        }

    }
}
