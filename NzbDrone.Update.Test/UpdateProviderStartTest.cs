using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    class UpdateProviderStartTest
    {
        AutoMoqer mocker = new AutoMoqer();

        [SetUp]
        public void Setup()
        {
            mocker = new AutoMoqer();
        }

        [Test]
        public void should_stop_nzbdrone_service_if_installed()
        {
            mocker.GetMock<ServiceProvider>()
                .Setup(c => c.ServiceExist(ServiceProvider.NzbDroneServiceName))
                .Returns(true);

            //Act
            mocker.Resolve<UpdateProvider>().Start(null);

            //Assert
            mocker.GetMock<ServiceProvider>().Verify(c => c.Stop(ServiceProvider.NzbDroneServiceName), Times.Once());
            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_kill_nzbdrone_process_if_running()
        {
            var proccesses = Builder<ProcessInfo>.CreateListOfSize(2).Build();

            mocker.GetMock<ProcessProvider>()
                .Setup(c => c.GetProcessByName(ProcessProvider.NzbDroneProccessName))
                .Returns(proccesses);

            //Act
            mocker.Resolve<UpdateProvider>().Start(null);

            //Assert
            mocker.GetMock<ProcessProvider>().Verify(c => c.Kill(proccesses[0].Id), Times.Once());
            mocker.GetMock<ProcessProvider>().Verify(c => c.Kill(proccesses[1].Id), Times.Once());
            mocker.VerifyAllMocks();
        }
    }
}
