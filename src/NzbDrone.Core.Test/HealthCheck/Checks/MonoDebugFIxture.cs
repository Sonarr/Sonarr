using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using static NzbDrone.Core.HealthCheck.Checks.MonoDebugCheck;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MonoDebugFixture : CoreTest<MonoDebugCheck>
    {
        private void GivenHasStackFrame(bool hasStackFrame)
        {
            MonoOnly();

            Mocker.GetMock<StackFrameHelper>()
                  .Setup(f => f.HasStackFrameInfo())
                  .Returns(hasStackFrame);
        }

        [Test]
        public void should_return_warning()
        {
            GivenHasStackFrame(false);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok()
        {
            GivenHasStackFrame(true);

            Subject.Check().ShouldBeOk();
        }
    }
}