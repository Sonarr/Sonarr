using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Model;
using NzbDrone.Test.Common;
using NzbDrone.Test.Dummy;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ProcessProviderTests : TestBase<ProcessProvider>
    {

        [SetUp]
        public void Setup()
        {
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c => c.Kill());
        }

        [TearDown]
        public void TearDown()
        {
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c => c.Kill());
        }

        [TestCase(0)]
        [TestCase(123332324)]
        public void Kill_should_not_fail_on_invalid_process_is(int processId)
        {
            Subject.Kill(processId);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void GetById_should_return_null_if_process_doesnt_exist()
        {
            Subject.GetProcessById(1234567).Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(9999)]
        public void GetProcessById_should_return_null_for_invalid_process(int processId)
        {
            Subject.GetProcessById(processId).Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void Should_be_able_to_kill_procces()
        {
            var dummyProcess = StartDummyProcess();
            Subject.Kill(dummyProcess.Id);
            dummyProcess.HasExited.Should().BeTrue();
        }

        [Test]
        public void Should_be_able_to_start_process()
        {
            var startInfo = new ProcessStartInfo(DummyApp.DUMMY_PROCCESS_NAME + ".exe");


            Subject.GetProcessByName(DummyApp.DUMMY_PROCCESS_NAME).Should()
                .BeEmpty("Dummy process is already running");
            Subject.Start(startInfo).Should().NotBeNull();

            Subject.GetProcessByName(DummyApp.DUMMY_PROCCESS_NAME).Should()
                .HaveCount(1, "excepted one dummy process to be already running");
        }

        [Test]
        public void kill_all_should_kill_all_process_with_name()
        {
            var dummy1 = StartDummyProcess();
            var dummy2 = StartDummyProcess();

            Subject.KillAll(dummy1.ProcessName);

            dummy1.HasExited.Should().BeTrue();
            dummy2.HasExited.Should().BeTrue();
        }

        public Process StartDummyProcess()
        {
            var startInfo = new ProcessStartInfo(DummyApp.DUMMY_PROCCESS_NAME + ".exe");
            return Subject.Start(startInfo);
        }

        [Test]
        public void ToString_on_new_processInfo()
        {
            Console.WriteLine(new ProcessInfo().ToString());
            ExceptionVerification.MarkInconclusive(typeof(Win32Exception));
        }

    }
}
