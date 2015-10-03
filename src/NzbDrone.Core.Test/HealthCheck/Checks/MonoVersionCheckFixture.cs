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

        [Test]
        public void should_return_warning_when_mono_3_0()
        {
            GivenOutput("3.0.0.1");

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_mono_2_10_8()
        {
            GivenOutput("2.10.8.1");

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_mono_2_10_2()
        {
            GivenOutput("2.10.2");

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok_when_mono_3_2()
        {
            GivenOutput("3.2.0.1");

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mono_4_0()
        {
            GivenOutput("4.0.0.0");

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mono_3_2_7()
        {
            GivenOutput("3.2.7");

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mono_3_2_1()
        {
            GivenOutput("3.2.1");

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mono_3_6_1()
        {
            GivenOutput("3.6.1");

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mono_3_10()
        {
            GivenOutput("3.10");

            Subject.Check().ShouldBeOk();
        }
    }
}
