using NUnit;
using NUnit.Framework;

namespace NzbDrone.Core.Test.Framework
{
    public class TestBaseNunit
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
            ExceptionVerification.AssertNoError();
        }

    }
}
