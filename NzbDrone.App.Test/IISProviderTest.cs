using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AutoMoq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Providers;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class IISProviderTest
    {



        [Test]
        public void start_should_set_IISProccessId_property()
        {
            var mocker = new AutoMoqer();

            var configMock = mocker.GetMock<ConfigProvider>();
            configMock.SetupGet(c => c.IISExePath).Returns("NzbDrone.Test.Dummy.exe");

            mocker.Resolve<ProcessProvider>();

            var iisProvider = mocker.Resolve<IISProvider>();

            iisProvider.StartServer();

            iisProvider.IISProcessId.Should().NotBe(0);
        }

    }
}
