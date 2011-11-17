//using System;
//using System.Diagnostics;
//using FluentAssertions;
//using Moq;
//using NUnit.Framework;
//using Ninject;
//using NzbDrone.Common;
//using NzbDrone.Common.Model;
//using NzbDrone.Providers;
//using NzbDrone.Test.Common;
//using NzbDrone.Test.Dummy;

//namespace NzbDrone.App.Test
//{
//    [TestFixture]
//    public class IISProviderFixture : TestBase
//    {
//        [Test]
//        public void should_update_pid_env_varibles()
//        {
//            WithTempAsAppPath();

//            var dummy = StartDummyProcess();

//            Environment.SetEnvironmentVariable(EnviromentProvider.NZBDRONE_PID, "0");
//            Environment.SetEnvironmentVariable(EnviromentProvider.NZBDRONE_PATH, "Test");

//            Mocker.GetMock<ProcessProvider>()
//                .Setup(c => c.Start(It.IsAny<ProcessStartInfo>()))
//                .Returns(dummy);

//            Mocker.Resolve<IISProvider>().StartServer();
//        }

//        [Test]
//        public void should_set_iis_procces_id()
//        {
//            WithTempAsAppPath();
//            var dummy = StartDummyProcess();

//            Mocker.GetMock<ProcessProvider>()
//                .Setup(c => c.Start(It.IsAny<ProcessStartInfo>()))
//                .Returns(dummy);

//            //act
//            Mocker.Resolve<IISProvider>().StartServer();

//            //assert
//            Mocker.Resolve<IISProvider>().IISProcessId.Should().Be(dummy.Id);
//        }


//        public Process StartDummyProcess()
//        {
//            var startInfo = new ProcessStartInfo(DummyApp.DUMMY_PROCCESS_NAME + ".exe");
//            startInfo.UseShellExecute = false;
//            startInfo.RedirectStandardOutput = true;
//            startInfo.RedirectStandardError = true;
//            startInfo.CreateNoWindow = true;
//            return new ProcessProvider().Start(startInfo);
//        }

//    }
//}
