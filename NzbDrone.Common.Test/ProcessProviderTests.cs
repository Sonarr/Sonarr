// ReSharper disable InconsistentNaming

using System;
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
    public class ProcessProviderTests : TestBase
    {

        ProcessProvider _processProvider;

        [SetUp]
        public void Setup()
        {
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c => c.Kill());
            _processProvider = new ProcessProvider();
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
            _processProvider.Kill(processId);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void GetById_should_return_null_if_process_doesnt_exist()
        {
            _processProvider.GetProcessById(1234567).Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(9999)]
        public void GetProcessById_should_return_null_for_invalid_process(int processId)
        {
            _processProvider.GetProcessById(processId).Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void Should_be_able_to_kill_procces()
        {
            var dummyProcess = StartDummyProcess();
            _processProvider.Kill(dummyProcess.Id);
            dummyProcess.HasExited.Should().BeTrue();
        }

        [Test]
        public void Should_be_able_to_start_process()
        {
            var startInfo = new ProcessStartInfo(DummyApp.DUMMY_PROCCESS_NAME + ".exe");

            //Act/Assert
            _processProvider.GetProcessByName(DummyApp.DUMMY_PROCCESS_NAME).Should()
                .BeEmpty("Dummy process is already running");
            _processProvider.Start(startInfo).Should().NotBeNull();

            _processProvider.GetProcessByName(DummyApp.DUMMY_PROCCESS_NAME).Should()
                .HaveCount(1, "excepted one dummy process to be already running");
        }

        public Process StartDummyProcess()
        {
            var startInfo = new ProcessStartInfo(DummyApp.DUMMY_PROCCESS_NAME + ".exe");
            return _processProvider.Start(startInfo);
        }

        [Test]
        public void ToString_on_new_processInfo()
        {
            Console.WriteLine(new ProcessInfo().ToString());
        }

    }
}
