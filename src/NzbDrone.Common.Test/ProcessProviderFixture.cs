using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;
using NzbDrone.Test.Common;
using NzbDrone.Test.Dummy;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ProcessProviderFixture : TestBase<ProcessProvider>
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
            Process.GetProcessesByName(DummyApp.DUMMY_PROCCESS_NAME).ToList().ForEach(c =>
            {
                try
                {
                    c.Kill();
                }
                catch (Win32Exception ex)
                {
                    TestLogger.Warn(ex, "{0} when killing process", ex.Message);
                }
                
            });
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
            var process = StartDummyProcess();

            Thread.Sleep(500);

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should()
                   .BeTrue("one running dummy process");

            process.Kill();
            process.WaitForExit();

            Subject.Exists(DummyApp.DUMMY_PROCCESS_NAME).Should().BeFalse();
        }

        [Test]
        [Explicit]
        public void Should_be_able_to_start_powershell()
        {
            WindowsOnly();

            var tempDir = GetTempFilePath();
            var tempScript = Path.Combine(tempDir, "myscript.ps1");

            Directory.CreateDirectory(tempDir);

            File.WriteAllText(tempScript, "Write-Output 'Hello There'\r\n");

            try
            {
                var result = Subject.StartAndCapture(tempScript);

                result.Standard.First().Content.Should().Be("Hello There");
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
            {
                Assert.Fail("No Powershell available?!?");
            }
        }

        [Test]
        public void Should_be_able_to_start_python()
        {
            WindowsOnly();

            var tempDir = GetTempFilePath();
            var tempScript = Path.Combine(tempDir, "myscript.py");

            Directory.CreateDirectory(tempDir);

            File.WriteAllText(tempScript, "print(\"Hello There\")\r\n");

            try
            {
                var result = Subject.StartAndCapture(tempScript);

                result.Standard.First().Content.Should().Be("Hello There");
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
            {
                Assert.Inconclusive("No Python available");
            }
        }


        [Test]
        public void kill_all_should_kill_all_process_with_name()
        {
            var dummy1 = StartDummyProcess();
            var dummy2 = StartDummyProcess();

            Thread.Sleep(500);

            Subject.KillAll(DummyApp.DUMMY_PROCCESS_NAME);

            dummy1.HasExited.Should().BeTrue();
            dummy2.HasExited.Should().BeTrue();
        }

        private Process StartDummyProcess()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, DummyApp.DUMMY_PROCCESS_NAME + ".exe");
            return Subject.Start(path);
        }

        [Test]
        public void ToString_on_new_processInfo()
        {
            Console.WriteLine(new ProcessInfo().ToString());
            ExceptionVerification.MarkInconclusive(typeof(Win32Exception));
        }
    }
}
