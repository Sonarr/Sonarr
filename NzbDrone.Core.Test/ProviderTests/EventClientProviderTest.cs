// ReSharper disable RedundantUsingDirective

using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Xbmc;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EventClientProviderTest : SqlCeTest
    {
        [Test]
        public void SendNotification_true()
        {
            //Setup
            

            var header = "NzbDrone Test";
            var message = "Test Message!";
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Notification, It.IsAny<byte[]>())).Returns(true);

            //Act
            var result = Mocker.Resolve<EventClientProvider>().SendNotification(header, message, IconType.Jpeg, "NzbDrone.jpg", address);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void SendNotification_false()
        {
            //Setup
            

            var header = "NzbDrone Test";
            var message = "Test Message!";
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Notification, It.IsAny<byte[]>())).Returns(false);

            //Act
            var result = Mocker.Resolve<EventClientProvider>().SendNotification(header, message, IconType.Jpeg, "NzbDrone.jpg", address);

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void SendAction_Update_true()
        {
            //Setup
            

            var path = @"C:\Test\TV\30 Rock";
            var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", path);
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Action, It.IsAny<byte[]>())).Returns(true);

            //Act
            var result = Mocker.Resolve<EventClientProvider>().SendAction(address, ActionType.ExecBuiltin, command);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void SendAction_Update_false()
        {
            //Setup
            

            var path = @"C:\Test\TV\30 Rock";
            var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", path);
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Action, It.IsAny<byte[]>())).Returns(false);

            //Act
            var result = Mocker.Resolve<EventClientProvider>().SendAction(address, ActionType.ExecBuiltin, command);

            //Assert
            Assert.AreEqual(false, result);
        }
    }
}