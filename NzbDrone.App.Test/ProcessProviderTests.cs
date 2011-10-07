using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Providers;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ProcessProviderTests
    {
        ProcessProvider _processProvider;



        [SetUp]
        public void Setup()
        {
            _processProvider = new ProcessProvider();
        }

        [TestCase(0)]
        [TestCase(123332324)]
        public void Kill_should_not_fail_on_invalid_process_is(int processId)
        {
            _processProvider.Kill(processId);
        }

        [Test]
        public void GetById_should_return_null_if_process_doesnt_exist()
        {
            _processProvider.GetProcessById(1234567).Should().BeNull();
        }

        [Test]
        public void Should_be_able_to_kill_procces()
        {
            var dummyProcess = StartDummyProcess();
            _processProvider.Kill(dummyProcess.Id);
            dummyProcess.HasExited.Should().BeTrue();
        }


        public Process StartDummyProcess()
        {
            return Process.Start("NzbDrone.Test.Dummy.exe");
        }

    }
}
