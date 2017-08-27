using System;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MonoVersionCheckFixture : CoreTest<MonoVersionCheck>
    {
        private void GivenOutput(string version)
        {
            MonoOnly();

            Mocker.GetMock<IPlatformInfo>()
                  .SetupGet(s => s.Version)
                  .Returns(new Version(version));
        }

        
        [TestCase("4.6")]
        [TestCase("4.4.2")]
        [TestCase("4.6")]
        [TestCase("4.8")]
        [TestCase("5.0")]
        [TestCase("5.2")]
        [TestCase("5.4")]
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
        [TestCase("3.10")]
        [TestCase("4.0.0.0")]
        [TestCase("4.2")]
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
