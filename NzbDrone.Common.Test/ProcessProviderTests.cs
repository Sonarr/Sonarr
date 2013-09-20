using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;
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
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c =>
                {
                    c.Kill();
                    c.WaitForExit();
                });

            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).Should().BeEmpty();
        }

        [TearDown]
        public void TearDown()
        {
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c => c.Kill());
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
        public void Should_be_able_to_start_process()
        {
            var process = Subject.Start(Path.Combine(Directory.GetCurrentDirectory(), DummyApp.DUMMY_PROCCESS_NAME + ".exe"));

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should()
                   .BeTrue("excepted one dummy process to be already running");

            process.Kill();
            process.WaitForExit();

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should().BeFalse();
        }


        [Test]
        public void kill_all_should_kill_all_process_with_name()
        {
            var dummy1 = StartDummyProcess();
            var dummy2 = StartDummyProcess();

            Subject.KillAll(DummyApp.DUMMY_PROCCESS_NAME);

            dummy1.HasExited.Should().BeTrue();
            dummy2.HasExited.Should().BeTrue();
        }

        private Process StartDummyProcess()
        {
            return Subject.Start(DummyApp.DUMMY_PROCCESS_NAME + ".exe");
        }

        [Test]
        public void ToString_on_new_processInfo()
        {
            Console.WriteLine(new ProcessInfo().ToString());
            ExceptionVerification.MarkInconclusive(typeof(Win32Exception));
        }
    }
}
