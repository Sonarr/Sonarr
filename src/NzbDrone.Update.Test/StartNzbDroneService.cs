using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Test.Common;
using NzbDrone.Update.UpdateEngine;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    public class StartNzbDroneServiceFixture : TestBase<StartNzbDrone>
    {
        [Test]
        public void should_start_service_if_app_type_was_serivce()
        {
            const string targetFolder = "c:\\Sonarr\\";

            Subject.Start(AppType.Service, targetFolder);

            Mocker.GetMock<IServiceProvider>().Verify(c => c.Start(ServiceProvider.SERVICE_NAME), Times.Once());
        }

        [Test]
        public void should_start_console_if_app_type_was_service_but_start_failed_because_of_permissions()
        {
            const string targetFolder = "c:\\Sonarr\\";

            Mocker.GetMock<IServiceProvider>().Setup(c => c.Start(ServiceProvider.SERVICE_NAME)).Throws(new InvalidOperationException());

            Subject.Start(AppType.Service, targetFolder);

            Mocker.GetMock<IProcessProvider>().Verify(c => c.SpawnNewProcess("c:\\Sonarr\\Sonarr.Console.exe", "/" + StartupContext.NO_BROWSER, null), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
