using MbUnit.Framework;

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
            if (!Assert.IsFailurePending) ExceptionVerification.AssertNoError();
        }

    }
}
