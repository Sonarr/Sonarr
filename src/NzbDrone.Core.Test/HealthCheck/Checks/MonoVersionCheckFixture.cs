using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MonoVersionCheckFixture : CoreTest<MonoVersionCheck>
    {
        [SetUp]
        public void Setup()
        {
            MonoOnly();
        }

        private void GivenOutput(string version)
        {
            Mocker.GetMock<IRuntimeInfo>()
                  .SetupGet(s => s.RuntimeVersion)
                  .Returns(string.Format("{0} (tarball Wed Sep 25 16:35:44 CDT 2013)", version));
        }

        [TestCase("3.10")]
        [TestCase("4.0.0.0")]
        [TestCase("4.2")]
        [TestCase("4.6")]
        [TestCase("4.4.2")]
        public void should_return_ok(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeOk();
        }

        [TestCase("2.10.2")]
        [TestCase("2.10.8.1")]
        [TestCase("3.0.0.1")]
        [TestCase("3.2.0.1")]
        [TestCase("3.2.1")]
        [TestCase("3.2.7")]
        [TestCase("3.6.1")]
        [TestCase("3.8")]
        public void should_return_warning(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeWarning();
        }


        [TestCase("4.4.0")]
        [TestCase("4.4.1")]
        public void should_return_error(string version)
        {
            GivenOutput(version);

            Subject.Check().ShouldBeError();
        }
    }
}
