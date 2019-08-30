using System;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DotnetVersionCheckFixture : CoreTest<DotnetVersionCheck>
    {
        private void GivenOutput(string version)
        {
            WindowsOnly();

            Mocker.GetMock<IPlatformInfo>()
                  .SetupGet(s => s.Version)
                  .Returns(new Version(version));
        }

        [TestCase("4.7.2")]
        [TestCase("4.8")]
        public void should_return_ok(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeOk();
        }

        [TestCase("4.6.2")]
        [TestCase("4.7")]
        [TestCase("4.7.1")]
        public void should_return_notice(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeNotice();
        }

        public void should_return_warning(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeWarning();
        }

        [TestCase("4.5")]
        [TestCase("4.5.2")]
        [TestCase("4.6.1")]
        public void should_return_error(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_ok_for_net462_on_Win1511()
        {
            Mocker.GetMock<IOsInfo>()
                  .SetupGet(v => v.Version)
                  .Returns("10.0.14392");

            GivenOutput("4.6.2");

            Subject.Check().ShouldBeOk();
        }
    }
}
