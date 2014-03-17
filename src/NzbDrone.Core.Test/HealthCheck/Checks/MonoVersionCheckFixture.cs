using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Processes;
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
            Mocker.GetMock<IProcessProvider>()
                  .Setup(s => s.StartAndCapture("mono", "--version"))
                  .Returns(new ProcessOutput
                  {
                      Standard = new List<string>
                      {
                          String.Format("Mono JIT compiler version {0} (Debian {0}-8)", version),
                          "Copyright (C) 2002-2011 Novell, Inc, Xamarin, Inc and Contributors. www.mono-project.com"
                      }
                  });
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
        public void should_return_null_when_mono_3_2()
        {
            GivenOutput("3.2.0.1");

            Subject.Check().Should().BeNull();
        }

        [Test]
        public void should_return_null_when_mono_4_0()
        {
            GivenOutput("4.0.0.0");

            Subject.Check().Should().BeNull();
        }

        [Test]
        public void should_return_null_when_mono_3_2_7()
        {
            GivenOutput("3.2.7");

            Subject.Check().Should().BeNull();
        }

        [Test]
        public void should_return_null_when_mono_3_2_1()
        {
            GivenOutput("3.2.1");

            Subject.Check().Should().BeNull();
        }
    }
}
