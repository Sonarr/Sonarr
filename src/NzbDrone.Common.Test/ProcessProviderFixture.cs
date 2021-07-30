using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;
using NzbDrone.Test.Common;
using NzbDrone.Test.Dummy;

namespace NzbDrone.Common.Test
{
    [NonParallelizable]
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
        [Retry(3)]
        public void Should_be_able_to_start_process()
        {
            var process = StartDummyProcess();

            Thread.Sleep(500);

            var check = Subject.GetProcessById(process.Id);
            check.Should().NotBeNull();

            process.Refresh();
            process.HasExited.Should().BeFalse();

            process.Kill();
            process.WaitForExit();
            process.HasExited.Should().BeTrue();
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
        [Platform(Exclude = "MacOsX")]
        [Retry(3)]
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
            var processStarted = new ManualResetEventSlim();

            string suffix;
            if (OsInfo.IsWindows)
            {
                suffix = ".exe";
            }
            else
            {
                suffix = "";
            }

            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, DummyApp.DUMMY_PROCCESS_NAME + suffix);
            var process = Subject.Start(path, onOutputDataReceived: (string data) =>
            {
                if (data.StartsWith("Dummy process. ID:"))
                {
                    processStarted.Set();
                }
            });

            if (!processStarted.Wait(5000))
            {
                Assert.Fail("Failed to start process within 2 sec");
            }

            return process;
        }

        [Test]
        public void ToString_on_new_processInfo()
        {
            Console.WriteLine(new ProcessInfo().ToString());
            ExceptionVerification.MarkInconclusive(typeof(Win32Exception));
        }
    }
}
