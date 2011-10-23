using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ProcessProviderTests
    {
        private const string DummyProccessName = "NzbDrone.Test.Dummy";
        ProcessProvider _processProvider;

        [SetUp]
        public void Setup()
        {
            Process.GetProcessesByName(DummyProccessName).ToList().ForEach(c => c.Kill());
            _processProvider = new ProcessProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Process.GetProcessesByName(DummyProccessName).ToList().ForEach(c => c.Kill());
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

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(9999)]
        public void GetProcessById_should_return_null_for_invalid_process(int processId)
        {
            _processProvider.GetProcessById(processId).Should().BeNull();
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
            var startInfo = new ProcessStartInfo(DummyProccessName + ".exe");

            //Act/Assert
            _processProvider.GetProcessByName(DummyProccessName).Should()
                .BeEmpty("Dummy process is already running");
            _processProvider.Start(startInfo).Should().NotBeNull();

            _processProvider.GetProcessByName(DummyProccessName).Should()
                .HaveCount(1, "excepted one dummy process to be already running");
        }

        public Process StartDummyProcess()
        {
            var startInfo = new ProcessStartInfo(DummyProccessName + ".exe");
            return _processProvider.Start(startInfo);
        }

    }
}
