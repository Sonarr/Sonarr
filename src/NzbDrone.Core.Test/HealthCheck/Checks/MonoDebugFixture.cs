using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using static NzbDrone.Core.HealthCheck.Checks.MonoDebugCheck;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MonoDebugFixture : CoreTest<MonoDebugCheck>
    {
        private void GivenHasStackFrame(bool hasStackFrame)
        {
            Mocker.GetMock<StackFrameHelper>()
                  .Setup(f => f.HasStackFrameInfo())
                  .Returns(hasStackFrame);
        }

        [Test]
        public void should_return_ok_if_windows()
        {
            WindowsOnly();

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_if_not_debug()
        {
            MonoOnly();

            GivenHasStackFrame(false);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_log_warning_if_not_debug()
        {
            MonoOnly();

            GivenHasStackFrame(false);

            Subject.Check();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_ok_if_debug()
        {
            MonoOnly();

            GivenHasStackFrame(true);

            Subject.Check().ShouldBeOk();
        }
    }
}
